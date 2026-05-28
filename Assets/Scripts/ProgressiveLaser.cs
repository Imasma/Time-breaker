using UnityEngine;

public class LaserCannonParent : MonoBehaviour
{
    private enum LaserState { Idle, Extending, Retracting }

    [Header("Audio")]
    [SerializeField] private FMODUnity.EventReference deathSound;
    private FMOD.Studio.EventInstance deathSoundInstance;

    [Header("Visuels du Laser")]
    [SerializeField] private Material laserMaterial;
    [SerializeField] private float laserWidth = 0.1f;

    [Header("Paramètres de Tir")]
    [SerializeField] private Transform muzzlePoint; 
    [SerializeField] private float maxDistance = 100f;
    [SerializeField] private float speed = 10f; 
    [SerializeField] private float idleDuration = 1f;
    [SerializeField] private LayerMask collisionLayers;

    [Header("Logique de Mort (KillPlayer)")]
    public Transform respawnPoint;
    private Cheat playerCheat;
    private Transform playerTransform;

    private LineRenderer line;
    private LaserState state = LaserState.Idle;
    private float timer;
    private float currentLength;

    private void Awake()
    {
        line = GetComponentInChildren<LineRenderer>();
        if (line == null) line = gameObject.AddComponent<LineRenderer>();
        
        line.material = laserMaterial;
        line.startWidth = laserWidth;
        line.endWidth = laserWidth;

        line.positionCount = 2;
        line.enabled = false;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerCheat = player.GetComponent<Cheat>();
        }
    }

    private void Start()
    {
        if (respawnPoint == null)
        {
            GameObject foundRespawn = GameObject.FindWithTag("Respawn Point");
            respawnPoint = foundRespawn.transform;
        }

        if (muzzlePoint == null) muzzlePoint = transform;
    }

    private void Update()
    {
        switch (state)
        {
            case LaserState.Idle:
                timer += Time.deltaTime;
                if (timer >= idleDuration) SetState(LaserState.Extending);
                break;

            case LaserState.Extending:
                currentLength += speed * Time.deltaTime;
                if (CheckCollision() || currentLength >= maxDistance) SetState(LaserState.Retracting);
                UpdateLaserVisuel();
                break;

            case LaserState.Retracting:
                currentLength -= speed * Time.deltaTime;
                if (currentLength <= 0) SetState(LaserState.Idle);
                UpdateLaserVisuel();
                break;
        }
    }

    private bool CheckCollision()
    {
        return Physics.Raycast(muzzlePoint.position, muzzlePoint.forward, currentLength, collisionLayers);
    }

    private void UpdateLaserVisuel()
    {
        Vector3 origin = muzzlePoint.position;
        Vector3 direction = muzzlePoint.forward;
        Vector3 endPoint = origin + (direction * currentLength);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, currentLength, collisionLayers))
        {
            endPoint = hit.point;
        }

        line.SetPosition(0, origin);
        line.SetPosition(1, endPoint);

        CheckPlayerHit(origin, endPoint);
    }

    private void CheckPlayerHit(Vector3 start, Vector3 end)
    {
        float dist = Vector3.Distance(start, end);
        if (Physics.Raycast(start, (end - start).normalized, out RaycastHit hit, dist))
        {
            if (hit.collider.CompareTag("Player")) Kill();
        }
    }

    private void Kill()
    {
        if (playerCheat != null && !playerCheat.godMode && respawnPoint != null)
        {
            // --- LOGIQUE SON ---
            if (deathSoundInstance.isValid())
            {
                deathSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                deathSoundInstance.release();
            }
            deathSoundInstance = FMODUnity.RuntimeManager.CreateInstance(deathSound);
            deathSoundInstance.start();

            // --- TÉLÉPORTATION ---
            playerTransform.position = respawnPoint.position;
            Rigidbody rb = playerTransform.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = Vector3.zero;
        }
    }

    private void SetState(LaserState newState)
    {
        state = newState;
        timer = 0;
        if (state == LaserState.Extending) line.enabled = true;
        if (state == LaserState.Idle) { line.enabled = false; currentLength = 0; }
    }

    private void OnDestroy()
    {
        if (deathSoundInstance.isValid()) deathSoundInstance.release();
    }
}