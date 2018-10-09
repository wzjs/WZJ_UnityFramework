# #!/bin/bash

#参数判断
if [ $# != 2 ];then
echo "Params error!"
echo "Need two params: 1.path of project 2.name of ipa file"
exit
elif [ ! -d $1 ];then
echo "The first param is not a direct."
exit

fi

#工程路径
project_path=$1




#编译工程
cd $1
#清理#
xcodebuild  clean

xcodebuild || exit

xcodebuild archive -scheme Unity-iPhone -archivePath $2.xcarchive
xcodebuild -exportArchive -archivePath $2.xcarchive -exportPath $2.ipa -exportOptionsPlist "$1/../exportOptionsPlist.plist"

