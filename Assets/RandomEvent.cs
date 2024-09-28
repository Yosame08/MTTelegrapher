using System;
using System.Collections;
using UnityEngine;
using Random = System.Random;

public class RandomEvent : MonoBehaviour {
    private enum Event {
        Truck,
        Thunder,
        Size,
    }
    private bool _activate;
    private float _timer;
    private readonly float[] _nxtEvent = new float[(int)Event.Size];
    private Action[] _event;
    private readonly Random _random = new();
    
    public Transform truckWarn, thunderWarn, dim;
    public Transform truck;
    public CharInfo braver;
    public AnimManager animation;

    private void Awake() {
        _event = new Action[] {Truck, Thunder};
        GetNext(Event.Truck, 30, 50);
        GetNext(Event.Thunder, 50, 80);
    }

    private void Update() {
        if (!_activate) return;
        _timer += Time.deltaTime;
        for (int i = 0; i < (int)Event.Size; i++) {
            if (_timer >= _nxtEvent[i]) {
                _event[i]();
            }
        }
    }

    private void Truck() {
        int line = _random.Next(0, 14);
        Debug.Log("Truck Event on line" + line);
        StartCoroutine(TruckPrepare(line));
        GetNext(Event.Truck, 30, 50);
    }
    
    private void Thunder() {
        int x, y;
        GetBraverXY(out x, out y);
        x -= _random.Next(-1, 2);
        y -= _random.Next(-1, 2);
        Debug.Log("Thunder Event on (" + x + ", " + y + ")");
        StartCoroutine(ThunderPrepare(x, y));
        GetNext(Event.Thunder, 50, 80);
    }

    private void GetNext(Event e, int randomMin, int randomMax) {
        while (true) {
            _nxtEvent[(int)e] = _random.Next(randomMin, randomMax) + _timer;
            bool br = true;
            for (int i = 0; i < (int)Event.Size; ++i) {
                if (i == (int)e) continue;
                if (Math.Abs(_nxtEvent[i] - _nxtEvent[(int)e]) < 6) {
                    br = false;
                    break;
                }
            }
            if (br) break;
        }
    }

    private IEnumerator TruckPrepare(int line) {
        // set the position of truckWarn and truck
        bool hit = false;
        truckWarn.gameObject.SetActive(true);
        truck.gameObject.SetActive(true);
        dim.gameObject.SetActive(true);
        truckWarn.position = new Vector3(10.57f, 4.8f - 0.64f * line, 0);
        truck.position = new Vector3(5.44f, 4.8f - 0.64f * line, 0);
        SpriteRenderer dimSprite = dim.GetComponent<SpriteRenderer>();
        // scale of truckWarn from 0 to 1 in 5 secs
        float timer = 0;
        while (timer < 5) {
            timer += Time.deltaTime;
            truckWarn.position -= 2.178f * Time.deltaTime * Vector3.right;
            if (timer > 4) dimSprite.color = new Color(0, 0, 0, 0.6f*(timer-4));
            yield return null;
        }
        // Sprite truck move from the left of the screen to the right in 1 sec
        // the opacity of dim from 0 to 1 in 1 sec, too
        timer = 0;
        while (timer < 0.5) {
            timer += Time.deltaTime;
            truck.position -= 23.04f * Time.deltaTime * Vector3.right;
            // if truck and braver collide, call the function of braver hit
            // get the vector between truck and braver, if the distance is less than 32, call the function of braver hit
            Vector3 vec = braver.transform.position - truck.position;
            Debug.Log(vec.magnitude);
            if ((!hit) && vec.magnitude < 0.32) {
                // braver.ModifyProperty(CharInfo.Property.Hp, -100);
                animation.PlayTruckCollide(braver.transform.position);
                braver.AddLog("被大运创了100血");
                hit = true;
            }
            yield return null;
        }
        // make these invisible
        dimSprite.color = new Color(0, 0, 0, 0);
        truckWarn.gameObject.SetActive(false);
        truck.gameObject.SetActive(false);
        dim.gameObject.SetActive(false);
    }
    
    private IEnumerator ThunderPrepare(int centerX, int centerY) {
        // set the position of thunderWarn and thunder
        thunderWarn.gameObject.SetActive(true);
        dim.gameObject.SetActive(true);
        thunderWarn.position = new Vector3(0.32f + 0.64f * centerX, 0.32f + 0.64f * centerY);
        SpriteRenderer dimSprite = dim.GetComponent<SpriteRenderer>();
        // scale of thunderWarn from 0 to 1 in 5 secs
        float timer = 0;
        while (timer < 5) {
            timer += Time.deltaTime;
            thunderWarn.localScale = Vector3.one * (float)(timer * 1.92 / 5);
            if (timer > 4) dimSprite.color = new Color(0, 0, 0, 0.6f*(timer-4));
            yield return null;
        }
        // the opacity of dim from 0 to 1 in 1 sec, too
        Vector3 vec = braver.transform.position - thunderWarn.position;
        // If the distance in X axis or Y axis is less than 0.96:
        if (Math.Abs(vec.x) < 0.64 || Math.Abs(vec.y) < 0.64) {
            // braver.ModifyProperty(CharInfo.Property.Hp, -100);
            animation.PlayThunderStrike(braver.transform.position);
            braver.AddLog("装b遭雷劈，被劈了100血");
        }
        // make these invisible
        dimSprite.color = new Color(0, 0, 0, 0);
        thunderWarn.gameObject.SetActive(false);
        dim.gameObject.SetActive(false);
    }
    
    public void Activate() {
        _activate = true;
    }

    private void GetBraverXY(out int x, out int y) {
        x = (int)((braver.transform.position.x - 0.32) / 0.64);
        y = (int)((braver.transform.position.y - 0.32) / 0.64);
    }
}