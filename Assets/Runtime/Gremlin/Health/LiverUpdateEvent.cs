namespace LiverDie.Gremlin.Health
{
    public struct LiverUpdateEvent
    {
        public float NewLiver { get; private set; }

        public float LiverChange { get; private set; }

        public LiverUpdateEvent(float newLiver, float liverChange)
        {
            NewLiver = newLiver;
            LiverChange = liverChange;
        }
    }
}
