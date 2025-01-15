using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    public float speed = 25000.0f;
    public float rotationSpeed = 25.0f;
    public float score = 0f;
    private float health = 100.0f;

    public GameObject bulletPrefab;

    private GameObject healthText;
    private GameObject scoreText;

    private float shootInterval = 0.25f;
    private float shootIntervalMortar = 0.9f;
    private float currentShootIntervalLeft = 2.0f;
    private float currentShootIntervalRight = 2.0f;
    private float currentShootIntervalMortar = 2.0f;
    private Rigidbody rb;

    private float vertical;
    private float horizontal;
    private Vector2 lastRightStickDirection = Vector2.zero;

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        healthText = GameObject.Find("HealthText");
        scoreText = GameObject.Find("ScoreText");
        if (healthText)
        {
            healthText.GetComponent<TMP_Text>().text = "Health: " + health;
            scoreText.GetComponent<TMP_Text>().text = "Score: " + score;
        }
        else
        {
            Debug.LogWarning("No health text found.");
        }
    }

    void FixedUpdate()
    {
        GetHit(100);
        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");
        if (vertical < 0)
        {
            vertical /= 2;
        }
        Vector3 velocity = transform.forward * vertical * speed * Time.fixedDeltaTime;
        velocity.y = rb.linearVelocity.y;
        rb.linearVelocity = velocity;
        transform.Rotate(transform.up * horizontal * rotationSpeed * Time.fixedDeltaTime);
    }

    void Update()
    {

        if (Gamepad.current.rightStick.ReadValue().magnitude > 0.1)
        {
            lastRightStickDirection = Gamepad.current.rightStick.ReadValue();
        }

        if (currentShootIntervalLeft < shootInterval)
        {
            currentShootIntervalLeft += Time.deltaTime;
        }
        if (currentShootIntervalRight < shootInterval)
        {
            currentShootIntervalRight += Time.deltaTime;
        }
        if (currentShootIntervalMortar < shootIntervalMortar)
        {
            currentShootIntervalMortar += Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.E) || Gamepad.current.rightTrigger.wasPressedThisFrame)
        {
            if (currentShootIntervalRight < shootInterval)
            {
                return;
            }
            Quaternion rotation = transform.rotation;
            rotation *= Quaternion.Euler(0, 90, 0);
            GameObject bullet = Instantiate(bulletPrefab, transform.position + transform.forward + Vector3.up * 3f, rotation);
            bullet.GetComponent<BulletController>().damage = 80.0f;
            currentShootIntervalRight = 0.0f;
        }
        if (Input.GetKeyDown(KeyCode.Q) || Gamepad.current.leftTrigger.wasPressedThisFrame)
        {
            if (currentShootIntervalLeft < shootInterval)
            {
                return;
            }
            Quaternion rotation = transform.rotation;
            rotation *= Quaternion.Euler(0, -90, 0);
            GameObject bullet = Instantiate(bulletPrefab, transform.position + transform.forward + Vector3.up * 3f, rotation);
            bullet.GetComponent<BulletController>().damage = 80.0f;
            currentShootIntervalLeft = 0.0f;
        }

        if (Input.GetKeyDown(KeyCode.Space) || Gamepad.current.xButton.wasPressedThisFrame)
        {
            if (currentShootIntervalMortar < shootIntervalMortar)
            {
                return;
            }
            if (Gamepad.current.xButton.wasPressedThisFrame)
            {
                ShootMortar("gamepad");
            }
            else
            {
                ShootMortar();
            }
        }

    }

    private void ShootMortar(String input = "mouse")
    {
        GameObject plane = GameObject.Find("Plane");
        Ray ray;
        if (input == "mouse")
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane planeObj = new(Vector3.up, plane.transform.position);
            if (planeObj.Raycast(ray, out float distance))
            {
                Vector3 target = ray.GetPoint(distance);
                Vector3 direction = target - transform.position;
                direction.y = 0;
                Quaternion rotation = Quaternion.LookRotation(direction);
                rotation *= Quaternion.Euler(-5, 0, 0);
                GameObject bullet = Instantiate(bulletPrefab, transform.position + transform.forward + Vector3.up * 3f, rotation);
                bullet.GetComponent<BulletController>().damage = 30.0f;
                currentShootIntervalMortar = 0.0f;
            }
        }
        else
        {
            Vector3 rightStickDirection = new(lastRightStickDirection.x, 0, lastRightStickDirection.y);
            Quaternion rotation = Quaternion.LookRotation(rightStickDirection);
            rotation *= Quaternion.Euler(-5, gameObject.transform.rotation.y, 0);
            GameObject bullet = Instantiate(bulletPrefab, transform.position + transform.forward + Vector3.up * 3f, rotation);
            bullet.GetComponent<BulletController>().damage = 30.0f;
            currentShootIntervalMortar = 0.0f;
        }
    }

    private void GetHit(float damage = 35)
    {
        health -= damage;
        if (health <= 0)
        {
            health = 0;
        }
        healthText.GetComponent<TMP_Text>().text = "Health: " + health;
        if (health <= 0)
        {

            Destroy(gameObject);
            UnityEngine.SceneManagement.SceneManager.LoadScene("Map");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Loot"))
        {
            score++;
            scoreText.GetComponent<TMP_Text>().text = "Score: " + score;
            Destroy(collision.gameObject);
        }
        if (collision.gameObject.CompareTag("EnemyBullet"))
        {
            GetHit();
        }
    }
}
