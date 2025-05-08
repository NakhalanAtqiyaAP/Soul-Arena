using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HeroKnight : CharacterBase
{
    [Header("Movement Settings")]
    [SerializeField] private float m_speed = 4.0f;
    [SerializeField] private float m_jumpForce = 7.5f;
    [SerializeField] private float m_rollForce = 6.0f;
    [SerializeField] private bool m_noBlood = false;
    [SerializeField] private GameObject m_slideDust;

    [Header("Health and Damage")]
    public float m_damage = 10f;
    [SerializeField] private PlayerAttack playerAttack;
    [SerializeField] private float damageInterval = 0.5f;
    private float lastDamageTime = -Mathf.Infinity;

    [Header("Score")]
    public static int player1Score = 0;
    public static int player2Score = 0;
    public static int winningScore = 4;
    public static bool gameOver = false;

    [Header("Components")]
    private Sensor_HeroKnight m_groundSensor;
    private Sensor_HeroKnight m_wallSensorR1;
    private Sensor_HeroKnight m_wallSensorR2;
    private Sensor_HeroKnight m_wallSensorL1;
    private Sensor_HeroKnight m_wallSensorL2;
    private Animator m_animator;
    private Rigidbody2D m_body2d;

    private bool m_grounded = false;
    private bool m_rolling = false;
    private bool m_blocking = false;
    private bool m_isWallSliding = false;
    private bool m_wasGrounded = false;

    private int m_facingDirection = 1;
    private int m_currentAttack = 0;

    private float m_timeSinceAttack = 0.0f;
    private float m_delayToIdle = 0.0f;
    private float m_rollCurrentTime = 0.0f;
    private float m_rollDuration = 8.0f / 14.0f;

    private string horizontalAxis = "Horizontal";
    private string attackButton = "JoystickAttack";
    private string blockButton = "JoystickBlock";
    private string jumpButton = "JoystickJump";
    private string rollButton = "JoystickRoll";

    private KeyCode attackKey = KeyCode.J;
    private KeyCode blockKey = KeyCode.H;
    private KeyCode jumpKey = KeyCode.K;
    private KeyCode rollKey = KeyCode.L;

    protected override void Start()
    {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR1 = transform.Find("WallSensor_R1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR2 = transform.Find("WallSensor_R2").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL1 = transform.Find("WallSensor_L1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL2 = transform.Find("WallSensor_L2").GetComponent<Sensor_HeroKnight>();
        base.Start();
        gameObject.tag = "Player1";


        if (playerAttack != null)
        {
            playerAttack.damage = m_damage;
            playerAttack.gameObject.SetActive(false);
        }

    }

    void Update()
    {
        if (gameOver) return;

        m_timeSinceAttack += Time.deltaTime;

        if (m_rolling)
            m_rollCurrentTime += Time.deltaTime;
        if (m_rollCurrentTime > m_rollDuration)
            m_rolling = false;

        // Ground check
        m_wasGrounded = m_grounded;
        m_grounded = m_groundSensor.State();

        if (!m_wasGrounded && m_grounded)
            m_animator.SetBool("Grounded", true);
        if (m_wasGrounded && !m_grounded)
            m_animator.SetBool("Grounded", false);

        m_isWallSliding = (m_wallSensorR1.State() && m_wallSensorR2.State()) ||
                         (m_wallSensorL1.State() && m_wallSensorL2.State());
        m_animator.SetBool("WallSlide", m_isWallSliding);

        m_animator.SetFloat("AirSpeedY", m_body2d.linearVelocity.y);

        HandleMovement();
        HandleActions();
    }

    void HandleMovement()
    {
        float inputX = Input.GetAxis(horizontalAxis);

        // Keyboard fallback
        if (Input.GetKey(KeyCode.D)) inputX = 1f;
        else if (Input.GetKey(KeyCode.A)) inputX = -1f;

        if (inputX > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
            m_facingDirection = 1;
        }
        else if (inputX < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            m_facingDirection = -1;
        }

        if (!m_blocking && !m_rolling)
            m_body2d.linearVelocity = new Vector2(inputX * m_speed, m_body2d.linearVelocity.y);

        // Animation states
        if (Mathf.Abs(inputX) > Mathf.Epsilon)
        {
            m_delayToIdle = 0.05f;
            m_animator.SetInteger("AnimState", 1);
        }
        else
        {
            m_delayToIdle -= Time.deltaTime;
            if (m_delayToIdle < 0)
                m_animator.SetInteger("AnimState", 0);
        }

        // Fall animation
        if (!m_grounded && m_body2d.linearVelocity.y < -0.1f)
            m_animator.SetBool("Fall", true);
        else if (m_grounded)
            m_animator.SetBool("Fall", false);
    }

    void HandleActions()
    {
        bool attackPressed = Input.GetButtonDown(attackButton) || Input.GetKeyDown(attackKey);
        bool blockPressed = Input.GetButton(blockButton) || Input.GetKey(blockKey);
        bool jumpPressed = Input.GetButtonDown(jumpButton) || Input.GetKeyDown(jumpKey);
        bool rollPressed = Input.GetButtonDown(rollButton) || Input.GetKeyDown(rollKey);

        // Attack
        // Attack
        if (attackPressed && m_timeSinceAttack > 0.25f && m_grounded && !m_rolling)
        {
            m_currentAttack++;
            if (m_currentAttack > 3) m_currentAttack = 1;
            if (m_timeSinceAttack > 1.0f) m_currentAttack = 1;

            m_animator.SetTrigger("Attack" + m_currentAttack);
            m_timeSinceAttack = 0.0f;

            // Call EnableAttackCollider through animation event
            // Or directly:
            EnableAttackCollider();
            Invoke("DisableAttackCollider", 0.5f);
        }

        // Block
        if (!m_rolling && m_grounded)
        {
            m_blocking = blockPressed;
            m_animator.SetBool("Block", m_blocking);
            m_animator.SetBool("IdleBlock", m_blocking);
        }

        // Roll
        if (rollPressed && !m_rolling && !m_blocking && m_grounded && !m_isWallSliding)
        {
            m_rolling = true;
            m_rollCurrentTime = 0.0f;
            m_animator.SetTrigger("Roll");
            m_body2d.linearVelocity = new Vector2(m_facingDirection * m_rollForce, m_body2d.linearVelocity.y);
        }

        // Jump
        if (jumpPressed && m_grounded && !m_rolling && !m_blocking)
        {
            m_animator.SetTrigger("Jump");
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
            m_body2d.linearVelocity = new Vector2(m_body2d.linearVelocity.x, m_jumpForce);
            m_groundSensor.Disable(0.2f);
        }
    }
    void EnableAttackCollider()
    {
        if (playerAttack != null)
        {
            playerAttack.gameObject.SetActive(true);
            Debug.Log("HeroKnight attack collider enabled");
        }
    }
    void DisableAttackCollider()
    {
        if (playerAttack != null)
            playerAttack.gameObject.SetActive(false);
    }

    public override void TakeDamage(float damage)
    {
        if (CurrentHealth <= 0 || m_blocking || m_rolling) return;
        if (Time.time < lastDamageTime + damageInterval) return;

        lastDamageTime = Time.time;
        m_animator.SetTrigger("Hurt");

        // Panggil base class
        base.TakeDamage(damage);
    }

    protected override void Die()
    {
        m_animator.SetBool("noBlood", m_noBlood);
        m_animator.SetTrigger("Death");

        ScoreManager.Instance.AddScore(false); // Player 2 gets point
        if (ScoreManager.Instance.CheckGameOver())
        {
            GameOverController.Instance.ShowGameOver(ScoreManager.Instance.GetWinner());
        }
        else
        {
            Invoke("ReloadScene", 2f);
        }

        m_body2d.linearVelocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
    }

    private void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void AE_SlideDust()
    {
        Vector3 spawnPosition = m_facingDirection == 1 ?
            m_wallSensorR2.transform.position : m_wallSensorL2.transform.position;

        if (m_slideDust != null)
        {
            GameObject dust = Instantiate(m_slideDust, spawnPosition, transform.localRotation);
            dust.transform.localScale = new Vector3(m_facingDirection, 1, 1);
        }
    }
}