
public abstract class State
{
    public abstract void OnEnter();
    public abstract void OnExit();
    public virtual void OnVisualUpdate() { }
    public virtual void OnUpdate() { }
    public virtual void OnLateUpdate() { }
    public virtual void OnFixedUpdate() { }
}