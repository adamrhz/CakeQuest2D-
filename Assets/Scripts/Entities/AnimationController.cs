using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{


    [SerializeField] Animator anim;
    private bool Animate = true;
    Movement pm;
    bool isMoving = false;
    [SerializeField] float animationDeadZone = 0f;

    private void Awake()
    {

        pm = GetComponent<Movement>();
        //anim = GetComponent<Animator>();
        pm.LookAtEvent += LookAt;
    }


    public void ToggleAnimator(bool on)
    {
        Animate = on;
        if (!Animate)
        {
            isMoving = false;
            anim.SetBool("IsMoving", isMoving);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!Animate) { return; }
        if (!anim) { return; }
        bool _isMoving = pm.GetInput().magnitude > animationDeadZone;
        if (_isMoving != isMoving)
        {
            isMoving = _isMoving;
            anim.SetBool("IsMoving", isMoving);

        }
        if (isMoving)
        {
            anim.SetFloat("HorizontalMov", pm.GetInput().x);
            anim.SetFloat("VerticalMov", pm.GetInput().y);
        }


    }


    public void LookAt(Vector2 direction)
    {
        if (!Animate) { return; }
        if (!anim) { return; }
        anim.SetFloat("HorizontalMov", direction.x);
        anim.SetFloat("VerticalMov", direction.y);
    }

}
