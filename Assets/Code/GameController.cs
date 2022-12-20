using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControler : MonoBehaviour
{
    static GameControler m_GameController = null;
    float m_PlayerLife = 100.0f;
    float m_PlayerShield = 100.0f;
    FPPlayerController m_Player;//
    // Start is called before the first frame update
    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    public static GameControler GetGameController()
    {
        
        if(m_GameController == null)
        {
            m_GameController = new GameObject("GameController").AddComponent<GameControler>();
            GameControllerData l_GameControllerData = Resources.Load<GameControllerData>("GameControllerData");
            m_GameController.m_PlayerLife = l_GameControllerData.m_Life;
            m_GameController.m_PlayerShield = l_GameControllerData.m_Shield;
        }

        return m_GameController;
    }

    public static void DestroySingleton()
    {
        if(m_GameController != null)
        {
            GameObject.Destroy(m_GameController.gameObject);
        }
        m_GameController = null;
    }

    public void SetPlayerLife(float PlayerLife)
    {
        m_PlayerLife = PlayerLife;

    }

    public float GetPlayerLife()
    {
        return m_PlayerLife;
    }

    public void SetPlayerShield(float PlayerShield)
    {
        m_PlayerShield = PlayerShield;
    }

    public float GetPlayerShield()
    {
        return m_PlayerShield;
    }
    
    public FPPlayerController GetPlayer()
    {
        return m_Player;
    }

    public void SetPlayer(FPPlayerController Player)
    {
        m_Player = Player;
    }

    public void RestartGame()
    {
        m_Player.RestartGame();
    }
}
