using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Bandit : CharacterBase
{
    [Header("Movement Settings")]
    [SerializeField] private float m_speed = 4.0f;
    [SerializeField] private float m_jumpForce = 7.5f;
    [SerializeField] private bool m_noBlood = false;

    [Header("Health and Damage")]
    public float m_damage = 10f;
    [SerializeField] private PlayerAttack playerAttack;
    [SerializeField] private float damageInterval = 0.5f;
    private float lastDamageTime = -Mathf.Infinity;

    [Header("UI")]
    public Slider healthSlider;

    private Animator m_animator;
    private Rigidbody2D m_body2d;
    private Sensor_Bandit m_groundSensor;
    private bool m_grounded = false;
    private bool m_combatIdle = false;
    private bool m_blocking = false;

    private string horizontalAxis = "Joystick2Horizontal";
    private string attackButton = "Joystick2Attack";
    private string blockButton = "Joystick2Block";
    private string jumpButton = "Joystick2Jump";

    private KeyCode attackKey = KeyCode.B;
    private KeyCode blockKey = KeyCode.N;
    private KeyCode jumpKey = KeyCode.M;

    protected override void Start()
    {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_Bandit>();
        gameObject.tag = "Player2";
        base.Start();

        if (healthSlider != null)
            healthSlider.value = 1f;

        if (playerAttack != null)
        {
            playerAttack.damage = m_damage;
            playerAttack.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (HeroKnight.gameOver) return;

        // Ground check
        if (!m_grounded && m_groundSensor.State())
        {
            m_grounded = true;
            m_animator.SetBool("Grounded", m_grounded);
        }
        else if (m_grounded && !m_groundSensor.State())
        {
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
        }

        HandleMovement();
        HandleActions();
    }

    void HandleMovement()
    {
        float inputX = Input.GetAxis(horizontalAxis);

        // Keyboard fallback
        if (Input.GetKey(KeyCode.RightArrow)) inputX = 1f;
        else if (Input.GetKey(KeyCode.LeftArrow)) inputX = -1f;

        m_body2d.linearVelocity = new Vector2(inputX * m_speed, m_body2d.linearVelocity.y);

        // Flip character
        if (inputX > 0.1f)
            transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        else if (inputX < -0.1f)
            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        m_animator.SetFloat("AirSpeed", m_body2d.linearVelocity.y);

        // Animation states
        if (Mathf.Abs(inputX) > 0.1f)
            m_animator.SetInteger("AnimState", 2);
        else if (m_combatIdle)
            m_animator.SetInteger("AnimState", 1);
        else
            m_animator.SetInteger("AnimState", 0);
    }

    void HandleActions()
    {
        bool attackPressed = Input.GetButtonDown(attackButton) || Input.GetKeyDown(attackKey);
        bool blockPressed = Input.GetButton(blockButton) || Input.GetKey(blockKey);
        bool jumpPressed = Input.GetButtonDown(jumpButton) || Input.GetKeyDown(jumpKey);

        // Attack
        if (attackPressed && m_grounded && !m_blocking)
        {
            m_animator.SetTrigger("Attack");

            if (playerAttack != null)
            {
                playerAttack.gameObject.SetActive(true);
                EnableAttackCollider();
    Invoke("DisableAttackCollider", 0.5f);
            }
        }

        // Block
        if (m_grounded)
        {
            m_blocking = blockPressed;
            m_combatIdle = m_blocking;
            m_animator.SetBool("Block", m_blocking);
        }

        // Jump
        if (jumpPressed && m_grounded && !m_blocking)
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
            Debug.Log("Bandit attack collider enabled");
        }
    }
    void DisableAttackCollider()
    {
        if (playerAttack != null)
            playerAttack.gameObject.SetActive(false);
    }

    public override void TakeDamage(float damage)
    {
        if (CurrentHealth <= 0 || m_blocking) return;
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

        ScoreManager.Instance.AddScore(true); // Player 1 gets point
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
}