#!/usr/bin/env python3

## Launch file: roslaunch rotors_gazebo mav_hovering_example.launch 

import rospy
import keyboard
import math
import bluepy.btle as btle
import json
import sys

from geometry_msgs.msg import PoseStamped
from crazys_arduino_controller.msg import GloveData

class PythonKeyboardJoystick(btle.DefaultDelegate):

    desired_x = 0
    desired_y = 0
    desired_z = 1
    desired_orientation_angle = 0
    desired_rotation = 0

    class ReadDelegate(btle.DefaultDelegate):
        tempArr = []
        def __init__(self, parent):
            self.parent = parent

        def handleNotification(self, cHandle, data):
            datastr = data.decode("utf-8")
            self.tempArr.append(datastr)
            if (datastr.find("}") != -1):
                jsonstr = "".join(self.tempArr)
                try:
                    self.parent.map_data(json.loads(jsonstr))
                except:
                    pass
                self.tempArr = []    

    def __init__(self, mode):
        self.pub = rospy.Publisher('/firefly/command/pose', PoseStamped, queue_size=10)
        if (mode == 'keyboard'):
            keyboard.on_press(self.onkeypress)
            rospy.spin()
        elif (mode == 'glove-bluetooth'):
            p = btle.Peripheral("19:27:30:4F:7C:05")
            p.withDelegate(self.ReadDelegate(self))
            print(f'Started CrazyS controller using {mode}')
            while not rospy.is_shutdown():
                while p.waitForNotifications(1) and not rospy.is_shutdown():
                    pass
            p.disconnect()
        elif (mode == 'glove-usb'):
            rospy.Subscriber("/CrazyS_arduino/glove_data", GloveData, self.glove_cb)
            rospy.spin()
        else:
            print("Invalid control mode.\nOptions: \n\n1. glove-bluetooth\n2. glove-usb\n3. keyboard\n")

    def glove_cb(self, data):
        if (data.pitch >= 25):
            self.desired_x -= 0.3
        elif (data.pitch <= -25):
            self.desired_x += 0.3
        if (data.roll >= 25):
            self.desired_y -= 0.3
        elif (data.roll <= -25):
            self.desired_y -= 0.3
        if (data.yaw >= 25):
            self.desired_orientation_angle -= 5
        elif (data.yaw <= -25):
            self.desired_orientation_angle += 5
        if (data.gesture != "Closed Grip"):
            if (data.elevation == 'UpFast'):
                self.desired_z += 0.2
            elif (data.elevation == 'DownFast'):
                self.desired_z -= 0.2
        self.publish_data()


    def map_data(self, data):
        if (data["pitch"] >= 25):
            self.desired_x += 0.3
        elif (data["pitch"] <= -25):
            self.desired_x -= 0.3
        if (data["roll"] >= 25):
            self.desired_y += 0.3
        elif (data["roll"] <= -25):
            self.desired_y -= 0.3
        if (data["yaw"] >= 25):
            self.desired_orientation_angle -= 5
        elif (data["yaw"] <= -25):
            self.desired_orientation_angle += 5
        if (data["currentGesture"] != "Closed Grip"):
            if (data["droneStatusText"] == 'UpFast'):
                self.desired_z += 0.2
            elif (data["droneStatusText"] == 'DownFast'):
                self.desired_z -= 0.2
        self.publish_data()

    def publish_data(self):
        imu_state = PoseStamped()
        imu_state.header.frame_id = 'arduino_controller'
        imu_state.header.stamp = rospy.Time.now()
        imu_state.pose.position.x = self.desired_x
        imu_state.pose.position.y = self.desired_y
        imu_state.pose.position.z = self.desired_z
        rad_angle = (self.desired_orientation_angle / 2) * math.pi / 180
        imu_state.pose.orientation.z = math.sin(rad_angle)
        imu_state.pose.orientation.w = math.cos(rad_angle)
        self.pub.publish(imu_state)

    def onkeypress(self, event):
        if (event.name == keyboard.KEY_UP):
            self.desired_x -= 0.1
        elif (event.name == keyboard.KEY_DOWN):
            self.desired_x += 0.1
        if (event.name == 'right'):
            self.desired_y += 0.1
        elif (event.name == 'left'):
            self.desired_y -= 0.1
        if (event.name == 'w'):
            self.desired_z += 0.1
        elif (event.name == 's'):
            self.desired_z -= 0.1
        if (event.name == 'q'):
            self.desired_orientation_angle += 2
        elif (event.name == 'e'):
            self.desired_orientation_angle -= 2
        self.publish_data()
 
if __name__ == '__main__':
    rospy.init_node('python_keyboard_joystick')
    try:
        mode = sys.argv[1]
        PythonKeyboardJoystick(mode)
    except IndexError:
        print("\nPlease specify drone control method.\nOptions: \n\n1. glove-bluetooth\n2. glove-usb\n3. keyboard\n")
