using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//general class for the cannon element on the game scene
//since it's only one cannon with general elements of the game multiple other elements need, it works as a singleton class
public class CannonScript : SingletonBehaviour<CannonScript>
{
    //general prefab for all projectile objects
    public GameObject projectilePrefab;
    //line renderer for the projectile projection
    public LineRenderer projectileLine;
    //sprite that shows landing spot for projctiles
    public Transform markSprite;

    //location of the firing point of the cannon
    public Vector3 muzzlePoint;
    //transform of the field element
    public Transform field;

    //barrel object of the cannon
    private Transform cannonBarrel;

    public bool loadedProjectile { get; private set; }
    private ProjectileScript.ProjectileType loadedType;

    //determines if the cannon base variables are set
    private bool cannonSet = false;

    //image knob to visualize cannon cooldown
    public Image cooldownGraphic;

    //cooldown time between shots
    private float shotCooldown;

    // Start is called before the first frame update
    void Start()
    {
        SetCannon();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!cannonSet)
            return;

        //key controls for the cannon actions
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            RotateCannon(false);
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            RotateCannon(true);

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            TiltCannon(true);
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            TiltCannon(false);

        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Keypad0))
            FireCannon();

        //we manage the cooldown
        if (shotCooldown > 0f)
        {
            shotCooldown -= Time.deltaTime;
            cooldownGraphic.fillAmount = shotCooldown / GameSettings.instance.shootCooldown;
        }
        else
        {
            cooldownGraphic.fillAmount = 0f;
            cooldownGraphic.transform.GetChild(0).gameObject.SetActive(false);
            shotCooldown = 0f;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.GetChild(0).TransformPoint(muzzlePoint), 0.06f);
    }

    //sets the cannon variables needed for correct functioning
    private void SetCannon()
    {
        cannonBarrel = transform.GetChild(0);

        cooldownGraphic.fillAmount = 0f;
        cooldownGraphic.transform.GetChild(0).gameObject.SetActive(false);
        shotCooldown = 0f;

        DrawTrajectory();

        cannonSet = true;
    }

    //fires next projectile in line. if no special projectile loaded, fires a default bouncing projectile
    private void FireCannon()
    {
        if (shotCooldown > 0f)
            return;

        GameObject nuProjectile = Instantiate<GameObject>(projectilePrefab);
        nuProjectile.transform.position = cannonBarrel.transform.position + muzzlePoint;
        ProjectileScript projScrip = nuProjectile.GetComponent<ProjectileScript>();

        //here we check the special bullets loaded
        ProjectileScript.ProjectileType nextType = loadedProjectile ? loadedType : ProjectileScript.ProjectileType.Bouncer;
        if (loadedProjectile)
            loadedProjectile = false;

        projScrip.Shoot(cannonBarrel.transform.position + muzzlePoint, GameSettings.instance.projectileStartSpeed * cannonBarrel.forward, nextType);

        //we set the cooldown
        cooldownGraphic.fillAmount = 1f;
        cooldownGraphic.transform.GetChild(0).gameObject.SetActive(true);
        shotCooldown = GameSettings.instance.shootCooldown;
    }

    //method that rotates the cannon. called each frame the respective key is pressed.
    private void RotateCannon(bool right)
    {
        Vector3 curRotation = transform.rotation.eulerAngles;
        float tempRotation = curRotation.y + ((right ? -1f : 1f) * GameSettings.instance.baseRotationSpeed * Time.deltaTime);
        if (tempRotation > 180f)
            tempRotation -= 360f;
        curRotation.y = Mathf.Clamp(tempRotation, GameSettings.instance.baseRotationLimits.x, GameSettings.instance.baseRotationLimits.y);
        transform.rotation = Quaternion.Euler(curRotation);

        DrawTrajectory();
    }

    //method that tilts the cannon. called each frame the respective key is pressed. I should've done it with vector and the transforms but it's too late for that now. Maybe later.
    private void TiltCannon(bool up)
    {
        Vector3 curRotation = cannonBarrel.localRotation.eulerAngles;
        float tempRotation = curRotation.x + ((up ? -1f : 1f) * GameSettings.instance.headRotationSpeed * Time.deltaTime);
        if (tempRotation > 180f)
            tempRotation -= 360f;
        curRotation.x = Mathf.Clamp(tempRotation, GameSettings.instance.headRotationLimits.x, GameSettings.instance.headRotationLimits.y);
        curRotation.y = 180f;
        curRotation.z = 0f;
        cannonBarrel.localRotation = Quaternion.Euler(curRotation);

        DrawTrajectory();
    }

    //ui controls for the cannon
    public void ControlPress(int directionID)
    {
        switch (directionID)
        {
            case 0:
                RotateCannon(false);
                break;
            case 1:
                RotateCannon(true);
                break;
            case 2:
                TiltCannon(true);
                break;
            case 3:
                TiltCannon(false);
                break;
        }
    }

    //ui control to fire the cannon
    public void FirePress()
    {
        FireCannon();
    }

    //method that loads a special bullet to the chamber
    public void LoadSpecialProjectile(ProjectileScript.ProjectileType type)
    {
        loadedProjectile = true;
        loadedType = type;
    }

    //method to visualize de trajectory of the projectile to be shot until it reaches ground level
    private void DrawTrajectory()
    {
        Vector3 pos = cannonBarrel.transform.position + muzzlePoint;
        Vector3 vel = GameSettings.instance.projectileStartSpeed * cannonBarrel.forward;
        Vector3 gravity = Physics.gravity;

        List<Vector3> trajectoryPoints = new List<Vector3>();

        float timeStep = GameSettings.instance.trajectoryVertexTimeStep;
        if (timeStep < 0f)
            timeStep = 0.01f; //just to make sure not to brick unity upon lowering values below zero

        while (true)
        {
            if (pos.y < field.position.y)
            {
                pos.y = 0.01f;
                markSprite.position = pos;
                break;
            }

            trajectoryPoints.Add(pos);

            vel = vel + (gravity * timeStep);
            pos = pos + (vel * timeStep);
        }

        projectileLine.positionCount = trajectoryPoints.Count;
        projectileLine.SetPositions(trajectoryPoints.ToArray());
    }

    
}
