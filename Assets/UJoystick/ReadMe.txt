Thanks for purchase UJoystick from Asset Store!

Requiered:
Unity 5.0.0++

Get Started:---------------------------------------------------------------------------------------------

- Import the package into your project.
- Drag the prefab "Joystick" located in: Assets/UJoystick/Content/Prefab/* and drop it in your Canvas root.
- If you need use more than one, do the same steps and just repositioned the UI.

How use:--------------------------------------------------------------------------------------------------
Order to use it for controller someting follow this steps:

- in the script that you have the code for controler eg: PlayerController,MouseLook,Move,etc...
- create a new reference / variable of 'bl_Joystick', example: 

    public bl_Joystick Joystick;

- the find the Input.GetAxis or the current input what you move the object (if you have one)
- and replace it with Joystick.Horizontal or Joystick.Vertical depends on you need.

Input.GetAxis("Vertical") = Joystick.Vertical
Input.GetAxis("Horizontal") = Joystick.Horizontal

- in case you using keys instead of axis (eg: Input.GetKey, due keys are bool and not float) you can do this:
- bool isKeyPressed = (Joystick.Horizontal > 0) ? true : false;

Tips:------------------------------------------------------------------------------------------------------

- Each Joystick provide two values (Vertical and Horizontal) order to use two diferent moves like:
one for rotate camera and other for move the player, you need two diferent joystick

Input.GetAxis("Vertical") and Input.GetAxis("Horizontal") can be take from a joystick
Input.GetAxis("Mouse Y") and Input.GetAxis("Mouse X") from other joystick

Contact:---------------------------------------------------------------------------------------------------
For any question or problem feel free for contact us.
Please if you have a problem, contact us before leave a bad review, we respond in no time.

Contact Form: http://www.lovattostudio.com/en/support/
