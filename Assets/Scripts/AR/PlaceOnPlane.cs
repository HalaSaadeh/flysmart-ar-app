using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;



public class PlaceOnPlane : MonoBehaviour
{
    #region Variables
    [Header("Game Objects")]
    public GameObject ar_camera;
    public GameObject visual_object;
    public GameObject placed_object;
    public GameObject ground;
    

    [System.NonSerialized] static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
    [System.NonSerialized] ARRaycastManager ray_manager;
    [System.NonSerialized] public bool is_object_placed;
    [System.NonSerialized] public float object_offset;
    [System.NonSerialized] public float ground_offset;
    #endregion
    void Awake()
    {
        ray_manager = FindObjectOfType<ARRaycastManager>();
        is_object_placed = false;
        object_offset = 0.5f;
        ground_offset = -0.5f;
        
    }

    bool TryGetTouchPosition(out Vector2 touchPosition)
    {
        if (Input.touchCount > 0 && !EventSystem.current.IsPointerOverGameObject())
        {
            touchPosition = Input.GetTouch(0).position;
            return true;
        }

        touchPosition = default;
        return false;
    }

    void Update()
    {
        if (!TryGetTouchPosition(out Vector2 touchPosition))
            return;

        if (ray_manager.Raycast(touchPosition, s_Hits, TrackableType.PlaneWithinPolygon))
        {
            // Raycast hits are sorted by distance, so the first one
            // will be the closest hit.
            //var hitPose = s_Hits[0].pose;
            var hitPose = visual_object.transform;
            Vector3 position = hitPose.position;
            Quaternion rotation = hitPose.rotation;

            position.y = position.y + object_offset;
            placed_object.transform.position = position ;
            placed_object.transform.rotation = ar_camera.transform.rotation;
            placed_object.transform.Rotate(0.0f, 90.0f, 0.0f);

            position = hitPose.position;
            rotation = hitPose.rotation;

            position.y = position.y + ground_offset;
            ground.transform.position = position;
            ground.transform.rotation = rotation;

            visual_object.SetActive(false);
            ground.SetActive(true);
            placed_object.SetActive(true);

            is_object_placed = true;

            
        }
    }




}