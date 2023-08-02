# Docker

## Docker architecture
    Docker uses a client-server architecture. The Docker client talks to the Docker daemon, which does the heavy lifting of building, running, and distributing your Docker containers. The Docker client and daemon can run on the same system, or you can connect a Docker client to a remote Docker daemon. The Docker client and daemon communicate using a REST API, over UNIX sockets or a network interface. Another Docker client is Docker Compose, that lets you work with applications consisting of a set of containers
![](./res/architecture.png)  
Docker是Client-Server架构,Client通过REST API发送命令(Docker Pull,Docker Build等)到Server(守护进程(daemon)),由Server来进行核心逻辑。另外一个核心是Registry,很多API(例如:Docker Pull),Server端都会去访问Registry去进行拉取,目前最大的Registry就是Docker Hub。

##


