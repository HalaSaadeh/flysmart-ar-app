# ROS Simulation

Gazebo simulation using the [CrazyS simulation](https://github.com/gsilano/CrazyS).

- *MPU9250*: Arduino sketch file.
- *msg/GloveData.msg*: Message file to send yaw, pitch, roll, elevation, and gesture.
- *scripts/arduino_controller.py*: Python script to map Arduino data to the drone. Modes: keyboard, glove-bluetooth, glove-usb.

