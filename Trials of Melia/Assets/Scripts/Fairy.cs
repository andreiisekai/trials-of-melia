using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Fairy : MonoBehaviour
{
    [SerializeField] float rcsThrust = 100f;
    [SerializeField] float mainThrust = 550f;
    [SerializeField] float levelLoadDelay = 2f;

    [SerializeField] AudioClip fairyFlying;
    [SerializeField] AudioClip death;
    [SerializeField] AudioClip success;

    [SerializeField] ParticleSystem fairyFlyingParticle;
    [SerializeField] ParticleSystem deathParticles;
    [SerializeField] ParticleSystem successParticles;

    Rigidbody rigidBody;
    AudioSource audioSource;
    bool collisionsDisabled = false;
    enum State { Alive, Dying, Transcending };
    State state = State.Alive;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (state == State.Alive)
        {
            RespondToThrustInput();
            RespondToRotateInput();
        }

        if (Input.GetKey(KeyCode.Escape)) 
        {
            Application.Quit();
        }

        if(Debug.isDebugBuild)
        {
            RespondToDebugKeys();
        }
    }

    private void RespondToDebugKeys()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadNextLevel();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            collisionsDisabled = !collisionsDisabled;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (state != State.Alive || collisionsDisabled)
        {
            return;   // ignore collision when dead
        }

        switch (collision.gameObject.tag)
        {
            case "Friendly":
                // do nothing
                break;
            case "Finish":
                StartSuccessSequence();
                break;
            default:
                StartDeathSequence();
                break;
        }
    }
    void StartSuccessSequence()
    {
        state = State.Transcending;
        playAudioUpon(success);
        successParticles.Play();
        Invoke("LoadNextLevel", levelLoadDelay);
    }
    void StartDeathSequence()
    {
        state = State.Dying;
        playAudioUpon(death);
        deathParticles.Play();  // Unity version Bug ?? Why does fairyFlyingParticle.Play() plays here alongside deathParticles.Play() only when space is kept being pressed ??
        Invoke("LoadFirstLevel", levelLoadDelay);
    }

    void playAudioUpon(AudioClip eventAudio)
    {
        audioSource.Stop();
        audioSource.PlayOneShot(eventAudio);
    }
    void LoadFirstLevel()
    {
        SceneManager.LoadScene(0);
    }

    void LoadNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;
        if (nextSceneIndex == SceneManager.sceneCountInBuildSettings)
        {
            nextSceneIndex = 0;  // loop back to first level
        }
        SceneManager.LoadScene(nextSceneIndex);      
    }

    void RespondToThrustInput()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            ApplyThrust();
        }
        else
        {
            StopApplyingThrust();
        }
    }

    private void StopApplyingThrust()
    {
        audioSource.Stop();
        fairyFlyingParticle.Stop();
    }

    void ApplyThrust()
    {
        float thrustThisFrame = mainThrust * Time.deltaTime;
        rigidBody.AddRelativeForce(Vector3.up * thrustThisFrame);
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(fairyFlying);
        }
        fairyFlyingParticle.Play();   // Unity version Bug ?? Why is this fairyFlyingParticle.Play() particle not starting to play when I press space ?
    }

    void RespondToRotateInput()
    {
        
        float rotationThisFrame = rcsThrust * Time.deltaTime;

        if (Input.GetKey(KeyCode.A))
        {
            RotateManually(rotationThisFrame);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            RotateManually(-rotationThisFrame);
        }

        
    }

    private void RotateManually(float rotationThisFrame)
    {
        rigidBody.freezeRotation = true; // take manual control of rotation
        transform.Rotate(Vector3.forward * rotationThisFrame);
        rigidBody.freezeRotation = false; // resume physics control of rotation
    }
}
