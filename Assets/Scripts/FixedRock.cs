using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class FixedRock : MonoBehaviour
{
    public GameObject rockPrefab;
    public Transform balanceBar;
    public Transform balancePoint;
    public GameObject uiPanel;
    public TMP_Text scoreText;
    public Button continueButton;

    private GameObject fixedRock;
    private GameObject fallingRock;
    private int level = 1;
    private float pressTime;
    private bool gameEnded = false;

    void Start()
    {
        SetupBalanceBar();
        SpawnFixedRock();
        uiPanel.SetActive(false);

        continueButton.onClick.AddListener(NextLevel);
    }

    void Update()
    {
        if (gameEnded) return;

        if (Input.GetMouseButtonDown(0))
        {
            pressTime = Time.time;
        }

        if (Input.GetMouseButtonUp(0))
        {
            float holdDuration = Time.time - pressTime;
            SpawnFallingRock(holdDuration);
        }

        if (fallingRock != null)
        {
            Rigidbody rb = fallingRock.GetComponent<Rigidbody>();
            if (rb.linearVelocity.magnitude < 0.1f && rb.IsSleeping())
            {
                EndGame();
            }
        }
    }

    void SetupBalanceBar()
    {
        Rigidbody rb = balanceBar.GetComponent<Rigidbody>() ?? balanceBar.gameObject.AddComponent<Rigidbody>();
        rb.mass = 20f;
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.angularDamping = 1f;

        HingeJoint hinge = balanceBar.GetComponent<HingeJoint>() ?? balanceBar.gameObject.AddComponent<HingeJoint>();
        hinge.anchor = Vector3.zero;
        hinge.axis = new Vector3(0, 0, 1);
        hinge.connectedBody = balancePoint.GetComponent<Rigidbody>();
    }

    void SpawnFixedRock()
    {
        if (fixedRock != null)
        {
            Destroy(fixedRock);
        }

        float size = Random.Range(0.5f, 1.5f + level * 0.2f);
        float weight = size * Random.Range(5f, 20f);

        Vector3 spawnPosition = balanceBar.position + new Vector3(-balanceBar.localScale.x / 2 + size / 2, 0.5f, 0);
        fixedRock = Instantiate(rockPrefab, spawnPosition, Quaternion.identity);
        fixedRock.transform.localScale = new Vector3(size, size, size);

        Rigidbody rb = fixedRock.GetComponent<Rigidbody>() ?? fixedRock.AddComponent<Rigidbody>();
        rb.mass = weight;
        rb.useGravity = false;
        rb.isKinematic = true;
    }

    void SpawnFallingRock(float holdDuration)
    {
        if (fallingRock != null) return;

        float size = Mathf.Clamp(holdDuration * 2f, 0.5f, 2.5f);
        float weight = size * Random.Range(5f, 20f);

        float rockX = fixedRock.transform.position.x;
        float spawnX = balanceBar.position.x * 2 - rockX;

        Vector3 spawnPosition = new Vector3(spawnX, balanceBar.position.y + 3f, 0);
        fallingRock = Instantiate(rockPrefab, spawnPosition, Quaternion.identity);
        fallingRock.transform.localScale = new Vector3(size, size, size);

        Rigidbody rb = fallingRock.GetComponent<Rigidbody>() ?? fallingRock.AddComponent<Rigidbody>();
        rb.mass = weight;
        rb.useGravity = true;
        rb.isKinematic = false;
    }

    void EndGame()
    {
        gameEnded = true;
        int score = CalculateScore();

        scoreText.text = "Điểm: " + score;
        uiPanel.SetActive(true);
    }

    int CalculateScore()
    {
        float angle = Mathf.Abs(balanceBar.rotation.eulerAngles.z);
        angle = (angle > 180) ? 360 - angle : angle;

        int score = Mathf.RoundToInt(100 - angle);
        return Mathf.Clamp(score, 0, 100);
    }

    public void NextLevel()
    {
        gameEnded = false;
        level++;
        uiPanel.SetActive(false);
        fallingRock = null;
        SpawnFixedRock();
    }
}
