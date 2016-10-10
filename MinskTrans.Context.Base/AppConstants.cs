namespace UniversalMinskTransRelease.Helpers
{
    static class AppConstatnsBase
    {
#if DEBUG
        public const string notificationHub = "minsktransnotificationhub";
#else
        public const string notificationHub = "minsktranshub_release";
#endif
    }

    public static class AppConstants
    {
        public const string HockeyAppId = "566821d63dcd405a8cdaaa5432f8cde3";
        public const string InsightsTelemetry = "21fc407a-013d-402a-b0de-6ab3157af6bf";
        public const string PushNotificationChanelHubName = AppConstatnsBase.notificationHub;
#if DEBUG
        public const string PushNotificationChanelEndPoint = "Endpoint=sb://minsktransnamespace.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=bd6RpWgvIKjEBTe00X46JgCX1PVjR4ZfXEwSzwIGHF4=";
#else
        public const string PushNotificationChanelEndPoint = "Endpoint=sb://minsktrans.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=ePQSPQTHljZ9KR6TyopcxQqF5EFkDnBpMb6aWb2krxA=";
#endif
        public const string GoogleAnalitics = "UA-85431708-1";
    }


    public static class AppServerConstants
    {
        public const string HockeyAppId = "237b48f97a63453da7fd07350f49c168";
        public const string InsightsTelemetry = "21fc407a-013d-402a-b0de-6ab3157af6bf";
        public const string PushNotificationChanelHubName = AppConstatnsBase.notificationHub;
#if DEBUG
        public const string PushNotificationChanelEndPoint = "Endpoint=sb://minsktransnamespace.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=FnUIrV6AXnVu/45Klip3lgdG2oNNSaofUtU7J7xEIXE=";
#else
        public const string PushNotificationChanelEndPoint = "Endpoint=sb://minsktrans.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=FNv7qjF3K1YUzTN/xyh1yXjBQzM/8b+VAKzUjoomwjk=";
#endif
        public const string GoogleAnalitics = "UA-85431708-1";
    }
}
