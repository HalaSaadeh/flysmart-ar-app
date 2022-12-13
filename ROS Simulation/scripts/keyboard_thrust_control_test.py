#!/usr/bin/env python3

import rospy
import keyboard
import time

from mav_msgs.msg import RollPitchYawrateThrustCrazyflie
from geometry_msgs.msg import Pose

class PythonKeyboardJoystick():
    def __init__(self):
        self.pub = rospy.Publisher('/crazyflie2/command/roll_pitch_yawrate_thrust', RollPitchYawrateThrustCrazyflie, queue_size=10)
        self.pos_sub = rospy.Subscriber('/crazyflie2/odometry_sensor1/pose', Pose, self.hover_cb)
        self.desired_pitch = 0
        self.desired_yaw_rate = 0
        self.desired_roll = 0
        self.desired_thrust = 6800
        keyboard.on_press(self.onkeypress)
        print('Started')
        self.hover()
        self.publishLoop()

    def hover_cb(self, data):
        altitude = data.position.z
        thrust_increase = 0
        if (altitude - 0.6 == 0):
            thrust_increase = 0
        else:
            thrust_increase = 1
        if (altitude < 0.5):
            if (self.desired_thrust > 6890):
                self.desired_thrust = 6890
            else:
                self.desired_thrust += thrust_increase
        elif (altitude > 0.7):
            if (self.desired_thrust < 6860):
                self.desired_thrust = 6860
            else:
                self.desired_thrust -= thrust_increase 

        print(f"Altitude: {altitude}")

    def hover(self):
        pass
    def publishLoop(self):
        while not rospy.is_shutdown():
            imu_state = RollPitchYawrateThrustCrazyflie()
            imu_state.header.frame_id = 'python_keyboard'
            imu_state.header.stamp = rospy.Time.now()
            imu_state.yaw_rate = self.desired_yaw_rate
            imu_state.pitch = self.desired_pitch
            imu_state.roll = self.desired_roll
            imu_state.thrust = self.desired_thrust
            self.pub.publish(imu_state)
            self.desired_pitch = 0
            self.desired_roll = 0
            time.sleep(0.1)

    def onkeypress(self, event):
        if (event.name == keyboard.KEY_UP):
            self.desired_pitch = 20
        elif (event.name == keyboard.KEY_DOWN):
            self.desired_pitch = -20
        if (event.name == 'right'):
            self.desired_roll = 20
        elif (event.name == 'left'):
            self.desired_roll = -20
        if (event.name == 'w'):
            self.desired_thrust += 100
        elif (event.name == 's'):
            self.desired_thrust -= 100

if __name__ == '__main__':
    rospy.init_node('python_keyboard_joystick')
    PythonKeyboardJoystick()
