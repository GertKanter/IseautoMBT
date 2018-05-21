killall gzclient
killall gzserver

sleep 5

killall -9 gzclient
killall -9 gzserver


../ros/src/util/packages/runtime_manager/scripts/gazebo.sh &

sleep 20

./initialpose.sh


