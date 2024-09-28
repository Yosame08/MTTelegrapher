using UnityEngine;

public class AnimManager : MonoBehaviour {
    public Animator enemyHit, braverHit, trap, getItem, randEvent;

    private static readonly int Hit = Animator.StringToHash("Hit"),
        Killed = Animator.StringToHash("Killed"),
        Punch = Animator.StringToHash("Punch"),
        Trapped = Animator.StringToHash("Trapped"),
        Get = Animator.StringToHash("Get"),
        Collide = Animator.StringToHash("Collide"),
        Thunder = Animator.StringToHash("Thunder");
    
    public void PlayEnemyHit(Vector3 position) {
        enemyHit.transform.position = position;
        enemyHit.SetTrigger(Hit);
    }
    
    public void PlayEnemyKilled(Vector3 position) {
        enemyHit.transform.position = position;
        enemyHit.SetTrigger(Killed);
    }
    
    public void PlayBraverHit(Vector3 position) {
        braverHit.transform.position = position;
        braverHit.SetTrigger(Hit);
    }
    
    public void PlayBraverPunch(Vector3 position) {
        braverHit.transform.position = position;
        braverHit.SetTrigger(Punch);
    }
    
    public void PlayBraverTrapped(Vector3 position) {
        trap.transform.position = position;
        trap.SetTrigger(Trapped);
    }
    
    public void PlayGetItem(Vector3 position) {
        getItem.transform.position = position;
        getItem.SetTrigger(Get);
    }
    
    public void PlayTruckCollide(Vector3 position) {
        randEvent.transform.position = position;
        randEvent.SetTrigger(Collide);
    }
    
    public void PlayThunderStrike(Vector3 position) {
        randEvent.transform.position = position;
        randEvent.SetTrigger(Thunder);
    }
}
