using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
/// <summary>
/// FileName: CamManager.cs
/// FileType: Visual C# Source file
/// Author : Ender Yıldız
/// Created On : 25/09/2022 11:30 AM
/// Last Modified On : 4/10/2022 11:49 AM
/// This class handles the touch controls of game. Move, pan, pinch etc.
/// Camera translates when pinch and zoom operations on Y axis.
/// </summary>
///

public class CameraMoveManager : MonoBehaviour
{
    
   

    [SerializeField] Transform CameraHolder;
    Camera MainCam;
    [SerializeField] int MinX;
    [SerializeField] int MaxX;
    [SerializeField] float MinZ;
    [SerializeField] float MaxZ;
    [SerializeField] int MinY;
    public int MaxY;


    [SerializeField] float CameraSpeed;
    [Tooltip("Higher will make longer camera sways.")]
    [SerializeField] public float DampingSpeed;
    [SerializeField] float PinchSpeed;
    [SerializeField] float MinPinchSpeed;
    Vector2 curDist;
    Vector2 prevDist;
    float touchDelta;
    float speedTouch0;
    float speedTouch1;
    float varianceInDistances;
    bool Brake = false;
    float CamHeightOffset;
    public Vector2 startPos;
    public Vector2 direction;
    [Tooltip("While UI operations camera movement can be off")]
    public bool CameraMovementDisabled
    {
        get;
        set;
    }

    public GameObject moveOnCam;
    int cameraUpMovement = 0;
    bool Pinching = false;
    public bool ImmediateStop = false;
    private void Start()
    {

        CameraHolder = GameObject.FindGameObjectWithTag("MainCamera").transform.parent;
        MainCam = CameraHolder.transform.GetChild(0).GetComponent<Camera>();
        
        UpdateCamera();
        CameraMovementDisabled = false;
    }


    public void MoveOnTween()
    {
        CameraHolder.transform.DOMove(new Vector3(moveOnCam.transform.position.x, CameraHolder.transform.position.y, moveOnCam.transform.position.z), 0.5f);
    }



    void Update()
    {
        if (!CameraMovementDisabled)
        {
            HandleMobileTouch();

        }
#if UNITY_EDITOR
        if (!CameraMovementDisabled)
        {
            HandleEditorControls();
        }
        

#endif

    }



    /// <summary>
    /// This function does it all. When touch count = 1 Handles the move and pan operations. Count =2 Handles the pinch  operation
    /// Camera frustum controls done.
    /// Camera is inside a holder for rotation changes.
    /// </summary>
    private void HandleMobileTouch()
    {
        if (Input.touchCount == 1) //Touch and pan NOT pinch so only one finger
        {
            if (ImmediateStop == true)
            {
                Invoke("ResumeCameraMovement", 1); // this value is camera move resume time after camera hits the base limit colliders. 1 is one second.
            }
            else
            {
                Touch touch = Input.GetTouch(0);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        startPos = touch.position;
                        break;

                    case TouchPhase.Moved:
                        
                            direction = touch.deltaPosition;
                            CameraHolder.transform.Translate(new Vector3(-touch.deltaPosition.x, 0, -touch.deltaPosition.y) * Time.deltaTime * CameraSpeed * (CamHeightOffset / 50));
                           
                                HoldCameraInWorldBoundries();
                            



                        
                        break;
                    case TouchPhase.Ended:
                        
                            Brake = true;
                        
                        break;
                }
            }
        }
        if (Input.touchCount == 2 && Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved)
        {
            Pinching = true;
            curDist = Input.GetTouch(0).position - Input.GetTouch(1).position; //current distance between finger touches
            prevDist = ((Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition) -
                (Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition)); //difference in previous locations using delta positions
            touchDelta = curDist.magnitude - prevDist.magnitude;
            speedTouch0 = Input.GetTouch(0).deltaPosition.magnitude / Input.GetTouch(0).deltaTime;
            speedTouch1 = Input.GetTouch(1).deltaPosition.magnitude / Input.GetTouch(1).deltaTime;
            //Pinch Out
            if ((touchDelta + varianceInDistances <= 1) && (speedTouch0 > MinPinchSpeed) && (speedTouch1 > MinPinchSpeed))
            {
                CameraHolder.Translate(Vector3.up * (PinchSpeed * (CamHeightOffset / 200)) * Time.deltaTime);

            }
            //Pinch In
            if ((touchDelta + varianceInDistances > 1) && (speedTouch0 > MinPinchSpeed) && (speedTouch1 > MinPinchSpeed))
            {
                CameraHolder.Translate(Vector3.down * (PinchSpeed * (CamHeightOffset / 100)) * Time.deltaTime);

            }
            
                HoldCameraInWorldBoundries();
             
            Brake = true;
            CamHeightOffset = CameraHolder.position.y;
        }
    }

    /// <summary>
    /// Late update runs after all other update functions. So camera will move last, after all other movements done.
    /// </summary>
    private void LateUpdate()
    {
        if (Brake)
        {
            if (Pinching ) //prevent the sway effect occurs for pinching and in city move. Only for swipe.
            {
                DampingSpeed = 0;
            }
            else
            {
                CameraHolder.transform.Translate(new Vector3(-direction.x, 0, -direction.y) * Time.deltaTime * CameraSpeed * DampingSpeed * (CamHeightOffset / 50));
                
                    HoldCameraInWorldBoundries();
                
                DampingSpeed = DampingSpeed - Time.deltaTime * 0.25f;
            }
            if (DampingSpeed <= 0.05f)
            {
                Brake = false;
                Pinching = false;
                DampingSpeed = 0.25f;
                CameraStopped();

            }
        }
    }

    public void UpdateCamera()
    {
        CameraHolder.position = new Vector3(CameraHolder.position.x, Mathf.Clamp(CameraHolder.position.y, MinY, MaxY), CameraHolder.position.z);
        
        Brake = true;
        CamHeightOffset = CameraHolder.position.y;
    }

   

    /// <summary>
    /// Clamps the camera position between bounds set on editor FOR WORLD.
    /// </summary>
    private void HoldCameraInWorldBoundries()
    {
        float FinalX = Mathf.Clamp(CameraHolder.transform.position.x, MinX, MaxX);
        float FinalZ = Mathf.Clamp(CameraHolder.transform.position.z, MinZ, MaxZ);
        float FinalY = Mathf.Clamp(CameraHolder.transform.position.y, MinY, MaxY);
        CameraHolder.transform.position = new Vector3(FinalX, FinalY, FinalZ);
    }


    

    /// <summary>
    /// Puts dummy objects at screen borders and calculates the camera frustums on real 3d World.
    /// </summary>
    int[] FindCoordinatesOfVisibleArea()
    {
        float depth = CameraHolder.transform.position.y;
        Vector3 upperLeftScreen = new Vector3(0, Screen.height, depth);
        Vector3 upperRightScreen = new Vector3(Screen.width, Screen.height, depth);
        Vector3 lowerLeftScreen = new Vector3(0, 0, depth);
        Vector3 lowerRightScreen = new Vector3(Screen.width, 0, depth);

        //Corner locations in world coordinates
        Vector3 upperLeft = MainCam.ScreenToWorldPoint(upperLeftScreen);
        Vector3 upperRight = MainCam.ScreenToWorldPoint(upperRightScreen);
        Vector3 lowerLeft = MainCam.ScreenToWorldPoint(lowerLeftScreen);
        Vector3 lowerRight = MainCam.ScreenToWorldPoint(lowerRightScreen);

        //upperLeft.y = upperRight.y = lowerLeft.y = lowerRight.y = ship.transform.position.y
        //Debug.Log(upperLeft.ToString()+" " + upperRight.ToString() + " " + lowerLeft.ToString() + " " + lowerRight.ToString());

        int[] Values = new int[4];
        Values[0] = (int)upperLeft.x; Values[1] = (int)upperRight.x; Values[2] = (int)lowerLeft.z; Values[3] = (int)upperRight.z;
        return Values;
    }

    /// <summary>
    /// Camera will stop smoothly
    /// </summary>
    void CameraStopped()
    {
        
            int[] val = FindCoordinatesOfVisibleArea();
        
        

    }
    
    public void MoveCameraToAnyPoint(Vector3 Pos)
    {
        MainCam.transform.parent.DOMove(Pos, 1.5f).OnComplete(SetCameraOffset);

    }
    void SetCameraOffset()
    {
        CamHeightOffset = CameraHolder.position.y;

    }

    void ResumeCameraMovement()
    {
        ImmediateStop = false;
    }
  
    void HandleEditorControls()
    {
        //CheckCameraIsInsideCityBorders();
        if (Input.GetKey(KeyCode.Q)) // up
        {

            //MainCam.transform.parent.Translate(Vector3.up * Time.deltaTime * 100);
            CameraHolder.Translate(Vector3.up * (PinchSpeed * (CamHeightOffset / 500)) * Time.deltaTime);

        }
        else if (Input.GetKey(KeyCode.Z))
        {
            //MainCam.transform.parent.Translate(Vector3.down * Time.deltaTime * 100);
            CameraHolder.Translate(Vector3.down * (PinchSpeed * (CamHeightOffset / 500)) * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.W))
        {
            MainCam.transform.parent.Translate(Vector3.forward * Time.deltaTime * CameraSpeed * 2);
        }
        else if (Input.GetKey(KeyCode.A))
        {
            MainCam.transform.parent.Translate(Vector3.left * Time.deltaTime * CameraSpeed * 2);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            MainCam.transform.parent.Translate(Vector3.back * Time.deltaTime * CameraSpeed * 2);
        }
        else if (Input.GetKey(KeyCode.D))
        {

            MainCam.transform.parent.Translate(Vector3.right * Time.deltaTime * CameraSpeed * 2);
        }
        if (Input.GetKeyUp(KeyCode.Q) || Input.GetKeyUp(KeyCode.Z))
        {
            Brake = true;
        }
        CameraHolder.position = new Vector3(CameraHolder.position.x, Mathf.Clamp(CameraHolder.position.y, MinY, MaxY), CameraHolder.position.z);
        SetCameraOffset();
    }
}