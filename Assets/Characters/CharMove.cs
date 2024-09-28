using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class CharMove : MonoBehaviour {
    private enum Status {
        Preparing,
        Move,
        Battle,
        GameOver
    }

    public static double actionTime => 10;

    private const float BlockSize = 0.64f;
    private float _moveSpeed = 4.8f;
    private bool _isMoving, _observerPrepared, _operatorPrepared;
    private Status _status;
    private int _wallLayer, _itemLayer, _trapLayer;
    private Coroutine _recoverCoroutine;
    private float _regenTimer;
    private float _gameTimer;
    private CharProperty _enemy;
    private int _prepared;
    private double _tmpSpeed;

    public CharInfo charInfo;
    public Transform battleBar;
    public Transform enemyBar;
    public Transform[] battleHelp;
    public TMP_Text timerText, enemyUltimate, operatorTitle, observerTitle;
    public AnimManager animations;
    // public RandomEvent randomEvent;

    private void Start() {
        _isMoving = false;
        SetStatus(Status.Preparing);
        _wallLayer = LayerMask.GetMask("Walls");
        _itemLayer = LayerMask.GetMask("Items");
        _trapLayer = LayerMask.GetMask("Traps");
    }

    private void Update() {
        if (_status == Status.Preparing) {
            if (Input.GetKey(KeyCode.Space) && !_observerPrepared) {
                operatorTitle.text = "已准备";
                _observerPrepared = true;
                _prepared += 1;
            }

            if (Input.GetKey(KeyCode.KeypadMultiply) && !_operatorPrepared) {
                observerTitle.text = "已准备";
                _operatorPrepared = true;
                _prepared += 1;
            }

            if (_prepared == 2) {
                operatorTitle.text = observerTitle.text = "游戏中";
                // randomEvent.Activate();
                SetStatus(Status.Move);
            }

            return;
        }

        _gameTimer -= Time.deltaTime;
        if (_gameTimer < 0) _gameTimer = 0;
        timerText.text = TimeSpan.FromSeconds(_gameTimer).ToString(@"mm\:ss\:ff");
        if (_gameTimer == 0) {
            charInfo.GameOver("时间到，游戏结束。", charInfo.property.score, 0);
            SetStatus(Status.GameOver);
            return;
        }

        _regenTimer += Time.deltaTime;
        if (_regenTimer >= 1f) {
            charInfo.Regenerate();
            _regenTimer -= 1f;
        }
        
        if (charInfo.property.hp <= 0) return;

        if (_status == Status.Move) {
            if (_isMoving) return;
            Vector3 direction = Vector3.zero;

            if (Input.GetKey(KeyCode.UpArrow)) {
                direction = Vector3.up;
            }
            else if (Input.GetKey(KeyCode.DownArrow)) {
                direction = Vector3.down;
            }
            else if (Input.GetKey(KeyCode.LeftArrow)) {
                direction = Vector3.left;
            }
            else if (Input.GetKey(KeyCode.RightArrow)) {
                direction = Vector3.right;
            }

            if (direction == Vector3.zero) return;
            bool willMove = true;
            Vector2 nextPosition = transform.position + direction * (BlockSize / 2);
            Collider2D colliderNow = Physics2D.OverlapCircle(nextPosition, 0.125f, _wallLayer);
            if (colliderNow) return;

            nextPosition = transform.position + direction * BlockSize;
            colliderNow = Physics2D.OverlapCircle(nextPosition, 0.125f, _itemLayer);
            if (colliderNow) {
                if (colliderNow.CompareTag("Crystal")) {
                    // 增加属性
                    animations.PlayGetItem(colliderNow.transform.position);
                    switch (colliderNow.name[0]) {
                        case 'A':
                            charInfo.ModifyProperty(CharInfo.Property.Atk, 2);
                            charInfo.AddLog("获得红宝石，攻击上升2");
                            break;
                        case 'D':
                            charInfo.ModifyProperty(CharInfo.Property.Def, 2);
                            charInfo.AddLog("获得蓝宝石，防御上升2");
                            break;
                        case 'S':
                            charInfo.ModifyProperty(CharInfo.Property.Speed, 2);
                            charInfo.AddLog("获得黄宝石，速度上升2");
                            break;
                        case 'R':
                            charInfo.ModifyProperty(CharInfo.Property.Regen, 1);
                            charInfo.AddLog("获得绿宝石，回血上升1");
                            break;
                        case 'H':
                            charInfo.ModifyProperty(CharInfo.Property.Hp, 100);
                            charInfo.AddLog("获得血瓶，生命增加100");
                            break;
                        case 'B':
                            charInfo.ModifyProperty(CharInfo.Property.Hp, 200);
                            charInfo.AddLog("获得血瓶，生命增加200");
                            break;
                        default:
                            throw new NotImplementedException("Crystal not implemented: " + colliderNow.name);
                    }

                    Destroy(colliderNow.gameObject);
                }
                else if (colliderNow.CompareTag("Key")) {
                    charInfo.AddLog("获得1把钥匙");
                    Destroy(colliderNow.gameObject);
                }
                else if (colliderNow.CompareTag("Enemy")) {
                    willMove = false;
                    _enemy = colliderNow.GetComponent<CharProperty>();
                    if (charInfo.property.def >= _enemy.atk) {
                        KillEnemy(_enemy, true);
                        Destroy(_enemy.gameObject);
                    }
                    else {
                        _tmpSpeed = _enemy.speed * 10 / charInfo.property.speed;
                        SetStatus(Status.Battle);
                        charInfo.AddLog("遭遇敌人，战斗开始");
                    }
                }
                else {
                    // 处理其他tag的事件
                    Debug.Log("Hit an object with tag: " + colliderNow.tag);
                }
            }

            if (willMove) {
                StartCoroutine(MoveToPosition(nextPosition));
            }

            colliderNow = Physics2D.OverlapCircle(nextPosition, 0.125f, _trapLayer);
            if (colliderNow) {
                if (colliderNow.CompareTag("Trap")) {
                    animations.PlayBraverTrapped(colliderNow.transform.position);
                    switch (colliderNow.name[0]) {
                        case 'H':
                            charInfo.ModifyProperty(CharInfo.Property.Hp, -100);
                            charInfo.AddLog("火焰陷阱，生命减少100");
                            break;
                        case 'S':
                            charInfo.AddLog("胶水陷阱，速度减慢15秒");
                            if (_recoverCoroutine != null) {
                                StopCoroutine(_recoverCoroutine);
                            }

                            SlowDown();
                            _recoverCoroutine = StartCoroutine(RecoverSpeedAfterDelay(15f));
                            break;
                        default:
                            throw new NotImplementedException("Trap not implemented: " + colliderNow.name);
                    }
                }
            }
        }
        else {
            charInfo.property.timer += Time.deltaTime * 10; // 自己固定一秒一次
            // 处理战斗状态
            if (charInfo.property.timer > actionTime) {
                charInfo.property.timer = actionTime;
                int action = 0;
                if (Input.GetKey(KeyCode.A)) action = 1;
                else if (Input.GetKey(KeyCode.S)) action = 2;
                else if (Input.GetKey(KeyCode.D)) action = 3;
                if (action != 0) charInfo.property.timer = 0;
                switch (action) {
                    case 1:
                        if (DamageHandle(charInfo.property, _enemy, out int finalDamage)) {
                            KillEnemy(_enemy, false);
                            SetStatus(Status.Move);
                            battleBar.localScale = new Vector3(0, 1, 1);
                            Destroy(_enemy.gameObject);
                            return;
                        }

                        animations.PlayEnemyHit(_enemy.transform.position);

                        charInfo.AddLog("造成" + finalDamage + "伤害，剩余" + _enemy.hp + "血");
                        break;
                    case 2:
                        charInfo.property.shield = true;
                        charInfo.AddLog("获得护盾，下次受伤大幅减少");
                        break;
                    case 3:
                        charInfo.AddLog("逃脱成功，战斗结束");
                        SetStatus(Status.Move);
                        battleBar.localScale = new Vector3(0, 1, 1);
                        return;
                }
            }

            battleBar.localScale = new Vector3((float)(charInfo.property.timer / actionTime), 1, 1);
            _enemy.timer += Time.deltaTime * _tmpSpeed;
            if (_enemy.timer >= actionTime) {
                _enemy.timer = 0;
                // 处理敌人的战斗状态
                double damage = Math.Max(_enemy.atk * (_enemy.coolDown == 0 ? 2 : 1) - charInfo.property.def, 0);
                if (charInfo.property.shield) {
                    damage = damage == 0 ? 0 : Math.Sqrt(damage);
                    charInfo.property.shield = false;
                }

                int finalDamage = (int)Math.Ceiling(damage);
                charInfo.AddLog((_enemy.coolDown == 0 ? "受到重击，血量-" : "受到攻击，血量-") + finalDamage);
                if (_enemy.coolDown == 0) {
                    _enemy.coolDown = _enemy.battle + 1;
                    animations.PlayBraverPunch(transform.position);
                }
                else animations.PlayBraverHit(transform.position);

                --_enemy.coolDown;
                enemyUltimate.text = "将在" + _enemy.coolDown + "次攻击后使用重击";
                if (!charInfo.ModifyProperty(CharInfo.Property.Hp, -finalDamage)) {
                    charInfo.AddLog("你招架不住对方的攻击，被弹开了");
                    charInfo.property.timer = 0;
                    battleBar.localScale = new Vector3(0, 1, 1);
                    SetStatus(Status.Move);
                }
            }

            enemyBar.localScale = new Vector3((float)(_enemy.timer / actionTime), 1, 1);
        }
    }

    private void SetStatus(Status status) {
        if (_status == Status.Preparing) _gameTimer = 900;
        _status = status;
        switch (status) {
            case Status.Move:
                foreach (var item in battleHelp) {
                    item.gameObject.SetActive(false);
                }

                break;
            case Status.Battle:
                foreach (var item in battleHelp) {
                    item.gameObject.SetActive(true);
                }

                enemyUltimate.text = "将在" + _enemy.coolDown + "次攻击后使用重击";

                break;
        }
    }

    private void SlowDown() {
        _moveSpeed = 3.2f;
    }

    private void SpeedRecover() {
        _moveSpeed = 4.8f;
    }

    private IEnumerator MoveToPosition(Vector3 target) {
        _isMoving = true;
        while ((target - transform.position).sqrMagnitude > Mathf.Epsilon) {
            transform.position = Vector3.MoveTowards(transform.position, target, _moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = target;
        _isMoving = false;
    }

    private IEnumerator RecoverSpeedAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        SpeedRecover();
    }

    /// <summary>
    /// 处理战斗事件，修改血量
    /// </summary>
    /// <returns>返回防守方是否被击败</returns>
    private bool DamageHandle(CharProperty attacker, CharProperty defender, out int finalDamage) {
        double damage = Math.Max(attacker.atk - defender.def, 0);
        if (defender.shield) {
            damage = damage == 0 ? 0 : Math.Sqrt(damage);
            defender.shield = false;
        }

        finalDamage = (int)Math.Ceiling(damage);
        defender.hp -= finalDamage;
        return defender.hp <= 0;
    }

    private void KillEnemy(CharProperty enemy, bool defKill) {
        charInfo.property.score += enemy.score;
        animations.PlayEnemyKilled(enemy.transform.position);
        charInfo.AddLog(defKill ? "防杀了" + enemy.charName : "击败" + enemy.charName + "，获得胜利");
        if (_enemy.GetComponent<Boss>()) {
            charInfo.AddLog("击败Boss！");
            charInfo.GameOver("击败Boss，恭喜通关！",
                _enemy.score + charInfo.property.hp + charInfo.property.regen * (int)_gameTimer,
                (int)_gameTimer);
            SetStatus(Status.GameOver);
        }
    }
}