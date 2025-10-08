namespace Characters.Player.Scripts
{
    public enum SwordCommand
    {
        Press, // Button pressed
        Release, // Button released
        Expire, // Timer expired
        Hit // Hit something
    }
    
    public class SwordStance
    {
        public HitboxType hitboxType;
        public string name;
        public bool canChangeDirection = false;
        public float transitionTime;
    }
}