using UnityEngine;

namespace Servus
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class FirstPersonMotor : MonoBehaviour
    {
        public Transform view;
        public float walkSpeed = 7f;
        public float runSpeed = 11f;
        public float jumpHeight = 1.3f;
        public float gravity = -25f;
        public float mouseSensitivity = 2f;
        CharacterController controller;
        Vector3 spawn;
        float verticalSpeed;
        float pitch;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            spawn = transform.position;
        }

        void Start() => CaptureCursor(true);
        void OnDisable() => CaptureCursor(false);
        void OnApplicationFocus(bool focused)
        {
            if (!focused) CaptureCursor(false);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) CaptureCursor(false);
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                if (Input.GetMouseButtonDown(0)) CaptureCursor(true);
                return;
            }

            transform.Rotate(0f, Input.GetAxisRaw("Mouse X") * mouseSensitivity, 0f);
            pitch = Mathf.Clamp(pitch - Input.GetAxisRaw("Mouse Y") * mouseSensitivity, -85f, 85f);
            view.localRotation = Quaternion.Euler(pitch, 0f, 0f);

            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            input = Vector2.ClampMagnitude(input, 1f);
            float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
            Vector3 movement = (transform.right * input.x + transform.forward * input.y) * speed;
            if (controller.isGrounded && verticalSpeed < 0f) verticalSpeed = -2f;
            if (controller.isGrounded && Input.GetKeyDown(KeyCode.Space))
                verticalSpeed = Mathf.Sqrt(jumpHeight * -2f * gravity);
            verticalSpeed += gravity * Time.deltaTime;
            movement.y = verticalSpeed;
            CollisionFlags collisions = controller.Move(movement * Time.deltaTime);
            if ((collisions & CollisionFlags.Above) != 0 && verticalSpeed > 0f) verticalSpeed = 0f;

            if (transform.position.y < -15f || Input.GetKeyDown(KeyCode.R))
            {
                controller.enabled = false;
                transform.SetPositionAndRotation(spawn, Quaternion.identity);
                controller.enabled = true;
                verticalSpeed = pitch = 0f;
                view.localRotation = Quaternion.identity;
            }
        }

        static void CaptureCursor(bool captured)
        {
            Cursor.lockState = captured ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !captured;
        }

        void OnGUI()
        {
            GUI.Label(new Rect(20, 18, 650, 30), "SERVUS / MOVEMENT PROTOTYPE");
            GUI.Label(new Rect(20, 46, 800, 30), "WASD: move   Mouse: look   Shift: run   Space: jump   R: reset   Esc: cursor");
            if (Cursor.lockState == CursorLockMode.Locked)
                GUI.Label(new Rect(Screen.width / 2f - 5, Screen.height / 2f - 10, 20, 25), "+");
            else
                GUI.Box(new Rect(Screen.width / 2f - 160, Screen.height / 2f - 25, 320, 50), "Click to play");
        }
    }
}
