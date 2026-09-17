using UnityEngine;

public class FlyCamera : MonoBehaviour
{
    /*
    Writen by Alexwing 2021, modify  Use it, edit it, steal it I don't care.  
    based in Windexglow    
    Simple flycam I made, since I couldn't find any others made public. 
    Made simple to use (drag and drop, done) for regular keyboard layout  
    Optional can anchor to terrain    
    WASD : basic movement
    SHIFT : Makes camera accelerate
    Mouse : Mouse look
    Scroll wheel : Height over terrain
    mouse click right: set lock/unlock movement         
	*/

    public static FlyCamera instance;

    [Tooltip("The terrain to follow")]
    public Terrain anchorToTerrain;
    private float marginToTerrain = 0;
    [Tooltip("The min height of the terrain to fly over")]
    public float minHeighToTerrain = 20f;
    [Tooltip("The max height of the terrain to fly over")]
    public float maxHeighToTerrain = 120f;

    [Tooltip("Regular speed")]
    public float mainSpeed = 2f;
    [Tooltip("Rotation speed")]
    public float rotationSpeed = 20f;
    [Tooltip(" Multiplied by how long shift is held.  Basically running.")]
    public float shiftAdd = 125f;
    [Tooltip("Maximum speed when holdin gshift")]
    public float maxShift = 100f;
    [Tooltip("How sensitive it with mouse")]
    public float camSens = 0.25f;
    private float totalRun = 1f;
    public static bool lockMovement = false;
    private float timeShiftLapsed = 1f;

    // Teleport-to-map-click state
    private bool _isTeleporting = false;
    private Vector3 _teleportTarget;
    [Tooltip("Minimum speed the camera glides to the map destination (units/sec)")]
    public float teleportSpeed = 400f;

    private void Awake()
    {
        instance = this;
        marginToTerrain = ((maxHeighToTerrain - minHeighToTerrain) * 0.5f) + minHeighToTerrain;
    }

    /// <summary>
    /// Smoothly moves the camera to a world-space XZ position (Y is kept terrain-anchored).
    /// Called by MapController when the player clicks on the minimap.
    /// </summary>
    public void TeleportTo(Vector3 worldPos)
    {
        _teleportTarget = worldPos;
        _isTeleporting = true;
    }

    private void Update()
    {
        // ── Teleport slide (overrides normal keyboard move while active) ──
        if (_isTeleporting)
        {
            Vector3 current = transform.position;
            Vector3 target = new Vector3(_teleportTarget.x, current.y, _teleportTarget.z);
            float dist = Vector3.Distance(new Vector3(current.x, 0, current.z), new Vector3(target.x, 0, target.z));
            float speed = Mathf.Max(teleportSpeed, dist * 3f);

            transform.position = Vector3.MoveTowards(current, target, speed * Time.deltaTime);
            if (dist < 2f)
            {
                _isTeleporting = false;
            }

            // Cancel teleport if player presses WASD movement keys
            if (GetBaseInput().sqrMagnitude > 0.001f)
            {
                _isTeleporting = false;
            }
        }
        else
        {
            Cursor.visible = lockMovement;
            Cursor.lockState = lockMovement ? CursorLockMode.None : CursorLockMode.Locked;

            if (!lockMovement)
            {
                // Mouse camera angle.  
                float h = Input.GetAxis("Mouse X") * rotationSpeed;
                float v = Input.GetAxis("Mouse Y") * rotationSpeed;
                Vector3 delta = new Vector3(h, v, 0f);

                delta = new Vector3(-delta.y * camSens, delta.x * camSens, 0f);
                delta = new Vector3(transform.eulerAngles.x + delta.x, transform.eulerAngles.y + delta.y, 0f);
                transform.eulerAngles = delta;

                // Keyboard commands
                Vector3 p = GetBaseInput();

                if (Input.GetKey(KeyCode.LeftShift))
                {
                    timeShiftLapsed += Time.deltaTime;
                    p = p * totalRun * shiftAdd * timeShiftLapsed;
                    p.x = Mathf.Clamp(p.x, -maxShift, +maxShift);
                    p.y = Mathf.Clamp(p.y, -maxShift, +maxShift);
                    p.z = Mathf.Clamp(p.z, -maxShift, +maxShift);
                }
                else
                {
                    totalRun = Mathf.Clamp(mainSpeed * 0.5f, 1f, 1000f);
                    p = p * totalRun;
                    timeShiftLapsed = 1f;
                }

                p = p * Time.deltaTime;
                transform.Translate(p);
            }
        }

        // ── Terrain anchoring (always active) ──
        if (anchorToTerrain)
        {
            Vector3 newPosition = transform.position;

            // Anchor to terrain.
            float height = anchorToTerrain.SampleHeight(this.transform.position);
            //block movement to terrain limits x and z
            newPosition.x = Mathf.Clamp(newPosition.x, anchorToTerrain.transform.position.x, anchorToTerrain.transform.position.x + anchorToTerrain.terrainData.size.x);
            newPosition.z = Mathf.Clamp(newPosition.z, anchorToTerrain.transform.position.z, anchorToTerrain.transform.position.z + anchorToTerrain.terrainData.size.z);
            this.transform.position = new Vector3(newPosition.x, height + marginToTerrain, newPosition.z);

            //mouse wheel change marginToTerrain
            if (!_isTeleporting)
            {
                marginToTerrain -= Input.GetAxis("Mouse ScrollWheel") * mainSpeed;
                if (minHeighToTerrain > marginToTerrain) marginToTerrain = minHeighToTerrain;
                if (marginToTerrain > maxHeighToTerrain) marginToTerrain = maxHeighToTerrain;
            }
        }

        //lock movement with mouse click
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonUp(1))
        {
            lockMovement = !lockMovement;
        }
    }

    private Vector3 GetBaseInput()
    {
        // Returns the basic values, if it's 0 than it's not active.
        Vector3 p_Velocity = new Vector3();
        if (!lockMovement)
        {
            if (Input.GetKey(KeyCode.W)) p_Velocity += Vector3.forward;
            if (Input.GetKey(KeyCode.S)) p_Velocity += Vector3.back;
            if (Input.GetKey(KeyCode.A)) p_Velocity += Vector3.left;
            if (Input.GetKey(KeyCode.D)) p_Velocity += Vector3.right;
        }
        return p_Velocity;
    }
}
