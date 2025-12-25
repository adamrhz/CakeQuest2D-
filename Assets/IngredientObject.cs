using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class IngredientObject : MonoBehaviour
{
    public bool Active = true;

    public MealIngredients IngredientData;
    public SpriteRenderer SR;
    public Image Image;
    public float LifeTime = 10f;
    public float deltaTime;

    public CookingManager Manager;

    public bool ReachDeathPoint = false;

    public void SetCookingManager(CookingManager manager)
    {
        Manager = manager;
    }
    public void SetIngredient(MealIngredients ingredient)
    {
        IngredientData = ingredient;
        if (SR) { SR.sprite = IngredientData.Icon; }
        if (Image) { Image.sprite = IngredientData.Icon; }

    }

    public void SetLifeTime(float lifeTime)
    {
        LifeTime = lifeTime;
    }

    public void DisableClick()
    {
        Destroy(GetComponent<UnityEngine.EventSystems.EventTrigger>());
    }

    public void AddGravity()
    {
        this.AddComponent<Rigidbody2D>();
    }


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Active)
        {
            UpdateLifeTime();
        }
    }

    private void UpdateLifeTime()
    {
        deltaTime += Time.deltaTime;
        if (LifeTime - deltaTime < .2f && !ReachDeathPoint)
        {
            ReachDeathPoint = true;
            this.ApplySquashAndStretch(1.5f, .2f);


        }
        if (deltaTime >= LifeTime)
        {
            Manager.SpawnPoof(this);
            Destroy(gameObject);
        }
    }

    public void OnClickedOn()
    {
        if (!Active) { return; }
        Debug.Log("Sprite clicked! Name: " + gameObject.name);
        Manager.IngredientPicked(IngredientData);
        Destroy(gameObject);
    }

    public void OnDestroy()
    {
        Manager?.ActiveIngredients?.Remove(this);
    }
}
