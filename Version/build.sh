
#!/bin/sh

#参数判断
if [ $# != 2 ];then
    echo "需要2个参数。"
    exit
    fi
    
    #UNITY程序的路径#
    UNITY_PATH=/Applications/Unity/Unity.app/Contents/MacOS/Unity
    
    #游戏程序路径#
    PROJECT_PATH=/Users/dongliang/Perforce/lizheng_dongliangdeMac-mini_2302/branches/OnlineVersion/Client/MagicP


    #IOS打包脚本路径#
    BUILD_IOS_PATH=/Users/dongliang/Version/buildios.sh
    
    #生成的Xcode工程路径#
    XCODE_PATH=/Users/dongliang/Version/$2/$1

    #将unity导出成xcode工程#
    $UNITY_PATH -projectPath $PROJECT_PATH -executeMethod ProjectBuild.BuildForIPhone $1 $2 -quit
    
    echo "XCODE工程生成完毕"
    
    #开始生成ipa#
    $BUILD_IOS_PATH $XCODE_PATH $1
    
    echo "ipa生成完毕"
