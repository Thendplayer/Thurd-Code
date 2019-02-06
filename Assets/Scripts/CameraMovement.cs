using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{

    [SerializeField] private bool v_static;
    [Space(10)]
    [SerializeField]
    private Transform v_target;
    [SerializeField] private Vector2 v_focusAreaSize;
    [Space(10)]
    [SerializeField]
    private float v_verticalOffset;
    [SerializeField] private float v_horizontalOffset;
    [SerializeField] public float v_smoothSpeed = 0.125f;

    [Space(10)]
    [SerializeField]
    private float v_maxCameraMovement;

    [HideInInspector] public bool v_active = true;

    [SerializeField] public List<Collider2D> L_zones = new List<Collider2D>();
    private Collider2D v_actualZone;
    private Collider2D v_lastZone;


    private bool v_canMoveLeft = true;
    private bool v_canMoveRight = true;
    private bool v_canMoveUp = true;
    private bool v_canMoveDown = true;

    private bool vAux_reFocus = false;

    private Player I_player;

    public struct SFocusArea
    {
        public Vector2 v_centre;
        public Vector2 v_velocity;
        private float v_left, v_right;
        private float v_top, v_bottom;

        public SFocusArea(Bounds targetBounds, Vector2 size)
        {
            v_left = targetBounds.center.x - size.x / 2;
            v_right = targetBounds.center.x + size.x / 2;
            v_bottom = targetBounds.min.y;
            v_top = targetBounds.min.y + size.y;

            v_velocity = Vector2.zero;
            v_centre = new Vector2((v_left + v_right) / 2, (v_top + v_bottom) / 2);
        }

        public void Update(Bounds targetBounds)
        {
            float _shiftX = 0;
            if (targetBounds.min.x < v_left)
            {
                _shiftX = targetBounds.min.x - v_left;
            }
            else if (targetBounds.max.x > v_right)
            {
                _shiftX = targetBounds.max.x - v_right;
            }
            v_left += _shiftX;
            v_right += _shiftX;

            float _shiftY = 0;
            if (targetBounds.min.y < v_bottom)
            {
                _shiftY = targetBounds.min.y - v_bottom;
            }
            else if (targetBounds.max.y > v_top)
            {
                _shiftY = targetBounds.max.y - v_top;
            }
            v_top += _shiftY;
            v_bottom += _shiftY;
            v_centre = new Vector2((v_left + v_right) / 2, (v_top + v_bottom) / 2);
            v_velocity = new Vector2(_shiftX, _shiftY);
        }
    }

    private SFocusArea i_focusArea;
    private Collider2D v_targetCollider;

    public bool v_staticNewPos = false;
    private Vector3 v_staticNewPosTarget;
    private float aux_currentHorizontalOffset;

    private float aux_originalHorizontalOffset;
    private float aux_originalVerticalOffset;

    private float aux_rightMoveTimer = 0.5f;
    private float aux_leftMoveTimer = 0.5f;

    void Awake()
    {
        v_targetCollider = v_target.GetComponent<Collider2D>();
        i_focusArea = new SFocusArea(v_targetCollider.bounds, v_focusAreaSize);
        v_actualZone = L_zones[0];

        I_player = v_target.GetComponent<Player>();
        aux_currentHorizontalOffset = v_horizontalOffset;

        aux_originalHorizontalOffset = v_horizontalOffset;
        aux_originalVerticalOffset = v_verticalOffset;
    }

    void Update()
    {
        if (!v_static)
        {
            aux_rightMoveTimer -= Time.deltaTime;
            aux_leftMoveTimer -= Time.deltaTime;

            if (Input.GetAxis("LookVertical") > 0)
                v_verticalOffset = Mathf.Lerp(v_verticalOffset, aux_originalVerticalOffset - v_maxCameraMovement, Time.deltaTime * 2.5f);
            else if (Input.GetAxis("LookVertical") < 0)
                v_verticalOffset = Mathf.Lerp(v_verticalOffset, -aux_originalVerticalOffset + v_maxCameraMovement, Time.deltaTime * 2.5f);
            else
                v_verticalOffset = Mathf.Lerp(v_verticalOffset, aux_originalVerticalOffset, Time.deltaTime * 5f);

            if (Input.GetAxis("LookHorizontal") > 0)
                aux_currentHorizontalOffset = Mathf.Lerp(aux_currentHorizontalOffset, aux_originalHorizontalOffset - v_maxCameraMovement, Time.deltaTime * 1.75f);
            else if (Input.GetAxis("LookHorizontal") < 0)
                aux_currentHorizontalOffset = Mathf.Lerp(aux_currentHorizontalOffset, -aux_originalHorizontalOffset + v_maxCameraMovement, Time.deltaTime * 1.75f);
            else
            {
                if (I_player.v_playerDirection == S_Direction.Right && aux_rightMoveTimer <= 0)
                {
                    aux_leftMoveTimer = 0.5f;
                    aux_currentHorizontalOffset = Mathf.Lerp(aux_currentHorizontalOffset, Mathf.Abs(v_horizontalOffset) * -1, Time.deltaTime * 1.6f);
                }
                else if (I_player.v_playerDirection == S_Direction.Left && aux_leftMoveTimer <= 0)
                {
                    aux_rightMoveTimer = 0.5f;
                    aux_currentHorizontalOffset = Mathf.Lerp(aux_currentHorizontalOffset, Mathf.Abs(v_horizontalOffset), Time.deltaTime * 1.6f);
                }
            }
        }
        else if (v_static && v_staticNewPos)
        {
            transform.position = Vector3.Lerp(transform.position, v_staticNewPosTarget, v_smoothSpeed);
            if (Vector3.Distance(transform.position, v_staticNewPosTarget) <= 0.1f)
            {
                transform.position = v_staticNewPosTarget;
                v_staticNewPos = false;
            }
        }
    }

    void FixedUpdate()
    {
        CheckStatus();

        if (v_active && !v_static)
        {
            i_focusArea.Update(v_targetCollider.bounds);

            Vector2 _focusPosition = i_focusArea.v_centre + (Vector2.up * v_verticalOffset) + (Vector2.left * aux_currentHorizontalOffset);
            Vector3 _newPos = (Vector3)_focusPosition + Vector3.forward * -10;

            _newPos = CanMove(_newPos);

            transform.position = Vector3.Lerp(transform.position, _newPos, v_smoothSpeed);
        }
    }

    public void ChangeStaticTarget(bool _isStatic, Vector3 _position)
    {
        if (_isStatic)
        {
            v_staticNewPos = _isStatic;
            v_static = _isStatic;
            v_staticNewPosTarget = new Vector3(_position.x, _position.y, this.transform.position.z); ;
        }
        else
        {
            v_static = _isStatic;
        }
    }

    public void ChangeStaticTarget(bool _isStatic)
    {
        v_static = _isStatic;
    }

    private void CheckStatus()
    {
        OutOfZone();

        DetectCurrentZone();
    }

    private void DetectCurrentZone()
    {
        foreach (Collider2D zone in L_zones)
        {
            if (zone.bounds.Contains(v_target.position))
            {
                if (v_actualZone != zone)
                {
                    v_lastZone = v_actualZone;
                    v_actualZone = zone;

                    vAux_reFocus = true;

                    Shield l_shield = I_player.i_shield;

                    if (l_shield.v_shieldState == S_ShieldState.Platform ||
                        l_shield.v_shieldState == S_ShieldState.Released)
                    {
                        l_shield.ChangeShieldState(l_shield.v_shieldState, S_ShieldState.Returning);
                    }
                }
            }
        }
    }

    private void OutOfZone()
    {
        Vector3 _maxX = new Vector3(v_actualZone.bounds.max.x, transform.position.y, 0); //>= 1 == dins
        Vector3 _minX = new Vector3(v_actualZone.bounds.min.x, transform.position.y, 0); //<= 0 == dins
        Vector3 _maxY = new Vector3(transform.position.x, v_actualZone.bounds.max.y, 0); //<= 1 == dins
        Vector3 _minY = new Vector3(transform.position.x, v_actualZone.bounds.min.y, 0); //<= 0 == dins

        v_canMoveLeft = Camera.main.WorldToViewportPoint(_minX).x > 0 ? false : true;
        v_canMoveRight = Camera.main.WorldToViewportPoint(_maxX).x < 1 ? false : true;
        v_canMoveUp = Camera.main.WorldToViewportPoint(_maxY).y > 1 ? true : false;
        v_canMoveDown = Camera.main.WorldToViewportPoint(_minY).y < 0 ? true : false;
    }

    private Vector3 CorrectPosition()
    {
        Vector3 _correctedPosition = new Vector3();

        float l_height = Camera.main.orthographicSize * 2;
        float l_width = l_height * Camera.main.aspect;

        if (v_actualZone.bounds.center.x < transform.position.x)
        {
            _correctedPosition = new Vector3(v_actualZone.bounds.max.x - l_width / 2, _correctedPosition.y, -10);
        }
        else
        {
            _correctedPosition = new Vector3(v_actualZone.bounds.min.x + l_width / 2, _correctedPosition.y, -10);
        }

        if (Mathf.Abs(_correctedPosition.x - transform.position.x) < .5f)
        {
            vAux_reFocus = false;
        }

        return _correctedPosition;
    }

    private Vector3 CanMove(Vector3 newPos)
    {
        Vector3 _newPos = newPos;

        if (newPos.x > transform.position.x && !v_canMoveRight)
        {
            _newPos = new Vector3(transform.position.x, _newPos.y, newPos.z);
        }

        if (newPos.x < transform.position.x && !v_canMoveLeft)
        {
            _newPos = new Vector3(transform.position.x, _newPos.y, newPos.z);
        }

        if (newPos.y > transform.position.y && !v_canMoveUp)
        {
            _newPos = new Vector3(_newPos.x, transform.position.y, newPos.z);
        }

        if (newPos.y < transform.position.y && !v_canMoveDown)
        {
            _newPos = new Vector3(_newPos.x, transform.position.y, newPos.z);
        }

        if (vAux_reFocus)
        {
            _newPos = new Vector3(CorrectPosition().x, _newPos.y, newPos.z);
        }

        return _newPos;
    }

    //**********************DEBUG***************************************************************************
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, .5f);
        Gizmos.DrawWireCube(i_focusArea.v_centre, v_focusAreaSize);
    }
}
