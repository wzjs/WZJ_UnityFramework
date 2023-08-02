# Docker
## Instruction
- FROM
  - FROM [--platform=\<platform>] \<image> [AS \<name>]
  - FROM [--platform=\<platform>] \<image>[:<tag>] [AS \<name>]
  - FROM [--platform=\<platform>] \<image>[@<digest>] [AS \<name>]  

  构建Dockerfile必须使用FROM指令来选择基础镜像  
  -platform:构建不同平台的镜像,默认构建宿主主机对应的平台。 例如你是x86平台,默认会构建x86,但是如果你想构建linux/arm64,就需要选择--platform=linux/arm64
  tag和digest都是在选择镜像特定的版本,tag是自定义标签,digest是哈希摘要  
  AS(stage name)用于为每个构建阶段定义一个名字,后续可以用该阶段处理一些事务。例如可以定义一个临时阶段用作编译程序,最后再将生成文件复制到最终阶段中

- RUN
  - RUN commond
  - RUN ["executable", "param1", "param2"]
  
  RUN有两种格式:  
  第一种Run 等同于 /bin/sh -c（Linux平台） || cmd /S /C (windows平台)  
  顺带一提像CMD,ENTRYPOINT都是类似,以下代码会输出/bin/sh -c "hello",这是一个非常好的论证,CMD "hello" 被作为参数输出了

        CMD "hello"
        ENTRYPOINT [ "echo" ]

  第二种可以不经过sh或者cmd,直接调用相应的程序,例如
  RUN ["usr/xxx.exe","1","2]  
  每次执行RUN时都会将结果提交到当前镜像,生成一个新的镜像用于之后的阶段,这样做可以在任何阶段都能继续创建镜像,这个是Docker的核心概念。另外RUN默认会缓存此次

  - RUN --mount
  - RUN --network
  - RUN --security

- CMD
  - CMD ["executable","param1","param2"] (exec form, this is the preferred form)
  - CMD ["param1","param2"] (as default parameters to ENTRYPOINT)
  - CMD command param1 param2 (shell form)
  
   CMD在运行docker之后会执行命令,如果Dockerfile中存在多条CMD命令,只会生效最后一条。  
   CMD的语法非常像RUN,第二条命令在设置了ENTRYPOINT之后,只代表参数  
   若在docker run 加上命令则会重载CMD,例如:docker run my_image echo "Override CMD"

- LABEL
  - LABEL \<key>=\<value> \<key>=\<value> \<key>=\<value> ...
  
  镜像的元数据,可以自定义记录一些镜像信息,可在后续镜像中查看该镜像的信息。
   docker image inspect --format='{{json .Config.Labels}}' myimage

- EXPOSE
  - EXPOSE \<port> [\<port>/\<protocol>...]
  
  只是一个标记没有实质作用(还是需要通过启动镜像时指定-p来映射端口),向使用者和开发者表明该镜像会开放哪些端口和协议


- ENV
  - ENV key=value key1=value key2=value
  - ENV key value (不支持单行多项,造成迷惑) 废弃 向下兼容 不鼓励使用
  
  ENV会在构建镜像时保存在镜像中,每次运行都会存在。如果只想在构建时使用可以使用ARG(不会保存在镜像),或者在单条命令中使用

        RUN DEBIAN_FRONTEND=noninteractive apt-get update && apt-get install -y ...
    或者直接在运行时指定环境变量,如果该key已经存在,则会覆盖先前定义的  
    
        docker run -e "key=value" -e "key=value" image
    另外基础镜像也会集成到当前镜像中,如果key重复,则子镜像会覆盖基础镜像

    使用ENV的方式有 $key ,\${key} ,\${key}可以用在一些没有空格的场景,例如\${test}_haha
    
    另外环境变量支持在环境变量中嵌套,例如
    
        ENV abc=hello
        ENV abc=bye def=$abc
        ENV ghi=$abc

    额外用法: ENV Key=\${variable:-word} Key1=\${variable:+word},variable是ARG定义的参数
    
    - -代表如果variable不存在,则使用word
    - +代表如果variable不存在,则使用空,存在的话就使用word  
  
  命令行:如果在shell中使用环境变量,那么

- ADD
   - ADD [--chown=\<user>:\<group>] [--chmod=\<perms>] [--checksum=\<checksum>] \<src>... \<dest>
    ADD [--chown=\<user>:\<group>] [--chmod=\<perms>] ["\<src>",... "\<dest>"]
    
   src为需要复制的文件或目录,可以是相对路径,绝对路径,URL,如果是一个压缩文件,则会自动解压到dest  
   src支持通配符,例如hell?.txt,*name.txt

   dest是容器中的目录,支持绝对路径和相对路径(WROKDIR)
   --chown 限制文件的用户和组
   --chmod 限制文件的权限

   规则:  
    1. 路径必须是在Context build内,不能获取其他路径,例如 ../something. 因为docker build时会将context build路径和子路径发送给daemon,所以上级目录是不支持的
    2. 如果src是一个url,dest没有以/结尾,那src的文件直接复制到dest中,如果dest是以/结尾,那么会去寻找合适的文件夹, 例如 www.xxx.com/folder, 那则会创建folder文件夹,将文件放入其中
    3. 如果src是一个文件夹,那么文件夹内的文件会被复制,自身则不会被复制
    4. dest加斜杆代表一个路径 src会复制到该目录下, 不加斜杆代表 src直接写入到 dest中
    5. 如果不存在dest,则会自动创建目录
    6. 多个源文件输入到dest中时,dest必须带斜杆,表示复制到一个目录下 
   
- COPY 
  
  1. 语法与ADD类似,但不支持自动解压
  2. 可以增加一个 --from=\<name>,关联 From image AS \<name>,可以将相应阶段作为目录
  3. COPY --link todo

- ENTRYPOINT
  - ENTRYPOINT ["executable", "param1", "param2"]
  - ENTRYPOINT command param1 param2

  > An ENTRYPOINT allows you to configure a container that will run as an executable.

  作用:官网的翻译指将容器封装成一个可执行体。即运行镜像时一定会执行该命令,同样是分为两种语法,一种是exec格式,一种是shell格式,就不多说了。

   说一下与CMD的区别:看过上面CMD的命令就会发现,CMD比ENTRYPOINT多了一种语法,就是不包含执行体,全部都是参数,这种语法就是为ENTRYPOINT准备的,当存在ENTRYPOINT时,CMD后续全部作为参数提供给ENTRYPOINT,若在运行docker run image 后指定了参数,则会覆盖CMD的参数.注意:ENTRYPOINT自带的参数不会被覆盖,除非--entrypoint arg1,arg2,另外其中有一个特例:当ENTRYPOINT为shell形式时,不管CMD为exec形式时还是docker run +参数覆盖,都不会成为ENTRYPOINT的参数。
  
  1. 如果CMD在base镜像已经定义过,ENTRYPOINT会清空CMD

- VOLUME
  - VOLUME ["/data"]
  
  作用:设置容器挂载点,就是说容器内的这个目录与宿主主机上的一个目录共享,哪边有改动另外一边也会改动。还可以用来在不同容器之间共享目录见 volume-from  
  注意:  
  1.  宿主目录是在镜像运行时才会生成,这是为了镜像的可移植性,因为我们不能保证在这之前宿主目录在不同平台的可用性。
   
- USER
  - USER \<user>[:\<group>]
  - USER \<UID>[:\<GID>]

  作用:能够在该阶段剩余的指令中使用该用户来执行,通常用在CMD,RUN等一些命令中,来降低用户的权限执行一些命令


- WORKDIR
  - WORKDIR /path/to/workdir
  
  作用:设置容器的当前目录,如果未设置则会使用base镜像的路径,如果base也未设置则默认使用\根目录。
  用法:使用多条WORKDIR时后一条会使用上一条的相对位置,例如
最后WORKDIR为 \a\b\c,另外可以配合环境变量一起使用。

      WORKDIR \a
      WORKDIR b
      WORKDIR c

- ARG
  - ARG \<name>[=\<default value>]
  
  作用: 在构建时指定参数,可以设置默认值,也可以通过命令行传递参数 --build-arg name=value。
  规则:   
   1. 若未在dockerfile中声明ARG,则--build-arg无效
   2. 声明时可以设置默认值,若未通过--build-arg传递,则使用默认值
   3. ARG声明在阶段内的话只能在那个阶段使用,如果声明在global,则全局可以使用
   
          ARG global=1
          FROM ubuntu
          ARG TEST=1
          FROM ubuntu
          ARG TEST=2
   4. ARG可以配合${variable:-word}和${variable:+word}使用,测试只有在设置变量的时候使用
   5. Predefined ARGs,docker预定义了了一些ARG,例如HTTP_PROXY,默认不会出现在docker history中,除非自身镜像覆盖, ARG HTTP_PROXY.另外在global scope中也定义了一些ARG,例如 TARGETPLATFORM,使用全局ARG,需要在阶段内声明才能使用
   6.  缓存丢失相关 --todo

- ONBUILD
   - ONBUILD \<INSTRUCTION>

  作用:在子镜像构建时调用, INSTRUCTION可以是大多数命令,例如RUN ADD COPY等。
  原理:类似于一个触发器在构建时检查包含的ONBUILD,将他们依次注册到元数据中(inspect可查看),只会在子镜像构建时(调用FROM时,如果指令出错,则FROM报错,所有触发器成功则继续执行FROM)触发。

- STOPSIGNAL signal  
    作用:用来指定当容器将要停止时给容器发送的退出信号,例如SIGKILL,默认使用SIGTERM,--stop-signal可以覆盖该指令

- HEALTHCHECK
  - HEALTHCHECK [OPTIONS] CMD command
  - HEALTHCHECK NONE 禁用父镜像继承下来的健康检查
  
  1. --interval=DURATION (default: 30s) 检查间隔
  2. --timeout=DURATION (default: 30s)  超时时间,若超过则此次检查失败
  3. --start-period=DURATION (default: 0s) 容器启动后的延迟时间
  4. --start-interval=DURATION (default: 5s) 在start period时间内的检查间隔
  5. --retries=N (default: 3) 最大尝试次数
  解释:根据options来设置参数,CMD commond需要返回0(success),1(unhealthy),2(reserved)来判断健康检查的结果,如果不健康的次数超过 --retries,则为不健康,会被记录在Docker inspect中

- SHELL
    SHELL ["executable","param1","param2"]

  作用:修改默认shell的程序及参数,Window默认为["cmd", "/S", "/C"],Linux默认为["/bin/sh", "-c"]。尤其是在Window上,因为Window上有两种shell,一种是cmd,一种是powershell。

---
exec和shell:这两种方式的实现在某些方面会造成不一样的影响,exec的执行体为第一进程,能够收到Unix的信号(只有PID1能收到信号),例如SIGTERM和SIGKILL,相关Unix信号自行搜索, shell的方式,shell本身为第一进程,所以执行体无法收到信号并处理。或者可以在 shell中添加 exec命令来替换成为PID1。实际在退出docker时docker会发送一个SIGTERM信号到容器,等待一段时间(默认10s)来供容器清理和退出,如果进程没有在规定时间内退出,docker会发送一个SIGKILL信号强制退出。


---
## Command Line
### Run
    docker run [OPTIONS] IMAGE[:TAG|@DIGEST] [COMMAND] [ARG...]
   作用: 启动镜像
   常用OPTIONS有:  
   -d(--detach):后台启动
   -e(--env):设置环境变量
   -i(--interactive) 打开容器的标准输入,配置-t,实现和终端交互
   -t(--tty) 在容器内创建一个伪终端,配合-i一起使用
   -v(--volume)  绑定挂载点

### Build
    docker build [OPTIONS] PATH | URL | -
  作用: 构建镜像, - 代表从标准输入中获取
  常用OPTIONS:
  -t(--tag):为构建的镜像指定一个标签,方便后续使用
  -build-arg: 传递参数到构件中
  --no-cache: 不使用缓存


---
### Docker 原理
  > https://developer.aliyun.com/article/981453  

   bootfs,rootfs,Union mount,image,layer
   Docker启动与Linux类似,都是用bootfs系统,bootfs(boot file system)包含了bootloader和kernel,bootloader用来引导启动kernel,启动kernel之后就会将bootfs卸载,并将内存使用权转交给内核。在内核引导时,它会加载一个小型临时文件系统,其中包含了系统所需要的文件和工具,然后linux使用这个文件系统来加载真正的根文件系统,可以是硬盘分区或者时NFS。  
   rootfs:linux的rootfs在加载完成之后一般是可读可写的,而docker的rootfs采用了镜像层(只读)+容器层(可读可写),容器层是在rootfs之上挂载一个新的文件系统,两个文件系统通过一定规则合并显示。例如当修改一个rootfs文件时,由于rootfs是有可读权限,kernel会复制该文件到容器层,修改的内容都保存在容器层中。这就是Union mount
   
   image:但是对于docker来说rootfs有个问题就是一个ubuntu系统200多MB,那每次构建都需要拉取这么大的镜像(如果没有缓存)或者每次稍微修改即使几KB都需要重新拉取,那效率是极其慢的,image非常巧妙的配合union mount解决了这个问题,image有一个类似继承的概念,每个镜像只包含该镜像直接需要的文件,其余依赖通过继承的方式获取,那么每个镜像只要划分的够细,镜像的大小就会非常小,即使要改变文件,也只是改变其中一个镜像,perfect！再加上镜像的本地缓存,效率之高！
   1.镜像之间没有冗余资源,优化了存储空间 2.不在需要拉取整套镜像,效率变快。 3.运行时存储变小,运行多个基于相同镜像层的容器,都只使用同一个镜像,不需要创建多份。

   layer:layer的概念和image比较接近,下面来自chatgpt。在这些层之上会新建一个可读可写层,容器如果需要写入,则会在这个层,而且这个层也可以被构建成一个镜像。例如:
   > 在 Docker 中，每个容器镜像都由一系列称为 "层"（Layers）的文件系统组成。Docker Layer 是 Docker 镜像的构建块，它们按顺序叠加在一起，形成一个完整的容器镜像。
    每一层都是只读的，并且包含了文件系统中的更改内容。当容器运行时，Docker 使用联合文件系统（UnionFS）将这些层叠加在一起，形成一个统一的文件系统视图，使得容器可以读取和写入这些文件。
    这种层的架构带来了一些优势：
    高效的镜像构建和分发：由于镜像的每个层都可以被缓存和共享，只有发生更改的层需要重新构建，因此构建和分发镜像的过程变得更加高效。
    版本控制和镜像复用：每个层都有一个唯一的标识符，因此可以方便地复用和管理镜像的不同版本。
    容器的轻量化：由于 Docker 层共享和重用相同的基础层，所以在创建多个容器时，只需增加或修改一小部分层即可，从而节省了存储空间和下载时间。
    需要注意的是，Docker 镜像的每一层都是只读的，因此容器在运行时无法直接修改镜像的内容。任何对容器进行的更改都会被写入一个新的可写层，这样可以确保镜像的不可变性和可重复性。
    总的来说，Docker Layer 是 Docker 镜像的基本组成部分，通过层叠加和联合文件系统的特性，使得 Docker 在资源有效性和容器管理方面具有很大的优势。
  
    
    




