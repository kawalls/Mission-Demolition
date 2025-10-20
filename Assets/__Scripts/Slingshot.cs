using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Slingshot : MonoBehaviour
{
    [Header("Inscribed")]
    public GameObject projectilePrefab;
    public GameObject projLinePrefab;
    public float velocityMult = 10f;

    public Transform leftArm;   // Reference to the LeftArm transform
    public Transform rightArm;  // Reference to the RightArm transform
    private LineRenderer slingshotLine; // Reference to the LineRenderer

    public AudioClip slingReleaseSound; // Your "sling-release.mp3" file
    public AudioSource audioSource;    // The "speaker" component

    [Header("Dynamic")]
    public GameObject launchPoint;
    public Vector3 launchPos;
    public GameObject projectile;
    public bool aimingMode;

    void Awake()
    {
        Transform launchPointTrans = transform.Find("LaunchPoint");
        launchPoint = launchPointTrans.gameObject;
        launchPoint.SetActive(false);
        launchPos = launchPointTrans.position;

        slingshotLine = GetComponent<LineRenderer>(); // Get the LineRenderer
        slingshotLine.enabled = false; // Disable it by default
        slingshotLine.positionCount = 3; // Set position count to 3 (L-Arm, Proj, R-Arm)
    }
    void OnMouseEnter()
    {
        // print("Slingshot:OnMouseEnter()");
        launchPoint.SetActive(true);
    }

    void OnMouseExit()
    {
        // print("Slingshot:OnMouseExit()");
        launchPoint.SetActive(false);
    }

    void OnMouseDown()
    {
        aimingMode = true;
        projectile = Instantiate(projectilePrefab) as GameObject;
        projectile.transform.position = launchPos;
        projectile.GetComponent<Rigidbody>().isKinematic = true;

        slingshotLine.enabled = true; // Enable the slingshot bands
    }

    void Update()
    {
        if (!aimingMode) return;

        Vector3 mousePos2D = Input.mousePosition;
        mousePos2D.z = -Camera.main.transform.position.z;
        Vector3 mousePos3D = Camera.main.ScreenToWorldPoint(mousePos2D);

        Vector3 mouseDelta = mousePos3D - launchPos;

        float maxMagnitude = this.GetComponent<SphereCollider>().radius;

        if (mouseDelta.magnitude > maxMagnitude)
        {
            mouseDelta.Normalize();
            mouseDelta *= maxMagnitude;
        }

        Vector3 projPos = launchPos + mouseDelta;
        projectile.transform.position = projPos;

        slingshotLine.SetPosition(0, leftArm.position);
        slingshotLine.SetPosition(1, projPos); // The middle point is the projectile
        slingshotLine.SetPosition(2, rightArm.position);

        if (Input.GetMouseButtonUp(0))
        {
            aimingMode = false;
            Rigidbody projRB = projectile.GetComponent<Rigidbody>();
            projRB.isKinematic = false;
            projRB.collisionDetectionMode = CollisionDetectionMode.Continuous;
            projRB.velocity = -mouseDelta * velocityMult;
            FollowCam.SWITCH_VIEW(FollowCam.eView.slingshot);
            FollowCam.POI = projectile;
            Instantiate<GameObject>(projLinePrefab, projectile.transform);
            projectile = null;
            MissionDemolition.SHOT_FIRED();

            slingshotLine.enabled = false;

            if (slingReleaseSound != null)
            {
                audioSource.PlayOneShot(slingReleaseSound);
            }
        }
    }

}
