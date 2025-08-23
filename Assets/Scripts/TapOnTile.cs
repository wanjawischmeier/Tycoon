using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;


namespace CameraActions
{
    // The awesome baseline for this: https://github.com/sergane13/Camera-Movement-By-Touch/blob/main/Assets/CameraMovementByTouch/Scripts/TapOnGameObject.cs
    public class TapOnTile : MonoBehaviour
    {
        #region "Input data"
        [Header("The sensitivity for counting a touch on the screen as a 'TOUCH'")]
        [SerializeField] private float _sensitivity;
        [SerializeField] private Tilemap _interactiveTilemap, _housingTilemap;
        [SerializeField] private Tile _highlightTile;
        [SerializeField] private HouseSettingsWindow _houseSettingsWindow;
        #endregion

        #region "Private members"
        private static Collider2D s_firstColliderTouched;

        private static bool s_first = false;
        private static bool s_second = false;

        private static bool s_hasMoved = false;

        private static bool cellHighlighted = false;
        private static Vector3Int previousCell;
        #endregion


        void Update()
        {
            if (Input.touchCount > 0 && !EventSystem.current.IsPointerOverGameObject(0)) // using the old input system
            {
                Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
                RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

                if (hit.collider != null)
                {
                    // Save the collider at the start of the touch
                    if (Input.GetTouch(0).phase == TouchPhase.Began)
                    {
                        s_firstColliderTouched = hit.collider;
                        s_first = true;
                        s_second = false;
                    }

                    // Check if the minimum touch deltaposition is more than _sensitivity to count the touch as MOVED
                    if (Input.GetTouch(0).phase == TouchPhase.Moved)
                    {
                        if (Mathf.Abs(Input.GetTouch(0).deltaPosition.x) > _sensitivity || Mathf.Abs(Input.GetTouch(0).deltaPosition.y) > _sensitivity)
                        {
                            s_hasMoved = true;
                        }
                    }

                    // Check if the end of touch is on the same collider 
                    if (Input.GetTouch(0).phase == TouchPhase.Ended)
                    {
                        if (s_firstColliderTouched == hit.collider && s_hasMoved == false)
                        {
                            s_second = true;
                        }
                        else
                        {
                            s_first = false;
                        }

                        s_hasMoved = false;
                    }


                    // if continions are true, the tap has begun on a colider, not moved withim the limits and the touch ended on the same collider
                    if (s_first && s_second)
                    {
                        s_first = false;
                        s_second = false;

                        // TODO: unneccessary ray, do correct conversion from worldPoint to rayPoint
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        Vector3 rayPoint = ray.GetPoint(-ray.origin.z / ray.direction.z);
                        Vector3Int cellPosition = _housingTilemap.WorldToCell(rayPoint);

                        if (cellHighlighted)
                        {
                            _interactiveTilemap.SetTile(previousCell, null);
                            _houseSettingsWindow.CloseBaseWindow();
                            cellHighlighted = false;
                        }

                        TileBase houseTile = _housingTilemap.GetTile(cellPosition);
                        if (houseTile != null)
                        {
                            _interactiveTilemap.SetTile(cellPosition, _highlightTile);
                            _houseSettingsWindow.OpenWindow(cellPosition);
                            previousCell = cellPosition;
                            cellHighlighted = true;
                        }
                    }
                }
            }
        }
    }
}