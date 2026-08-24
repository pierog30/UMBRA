using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerRespawn : MonoBehaviour
{
    public Vector3 respawnPoint;

    private Rigidbody2D rb;
    private int sceneIndex;

    private string SavePrefix => "UmbraCheckpoint_" + sceneIndex + "_";

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sceneIndex = SceneManager.GetActiveScene().buildIndex;
        respawnPoint = transform.position;

        if (PlayerPrefs.GetInt(SavePrefix + "Valid", 0) == 1)
        {
            respawnPoint = new Vector3(
                PlayerPrefs.GetFloat(SavePrefix + "X", respawnPoint.x),
                PlayerPrefs.GetFloat(SavePrefix + "Y", respawnPoint.y),
                respawnPoint.z);
            transform.position = respawnPoint;
        }
    }

    public void SetCheckpoint(Vector3 point)
    {
        respawnPoint = point;
        PlayerPrefs.SetInt(SavePrefix + "Valid", 1);
        PlayerPrefs.SetFloat(SavePrefix + "X", point.x);
        PlayerPrefs.SetFloat(SavePrefix + "Y", point.y);
        PlayerPrefs.Save();
    }

    public void Respawn()
    {
        rb.linearVelocity = Vector2.zero;
        transform.position = respawnPoint;
    }

    public void Die()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerDied(this);
            return;
        }

        Respawn();
    }

    public static void ClearSavedCheckpoint(int index)
    {
        string prefix = "UmbraCheckpoint_" + index + "_";
        PlayerPrefs.DeleteKey(prefix + "Valid");
        PlayerPrefs.DeleteKey(prefix + "X");
        PlayerPrefs.DeleteKey(prefix + "Y");
    }
}
