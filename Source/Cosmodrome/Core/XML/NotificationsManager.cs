using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace RocketMan
{
    public enum NotificationType
    {
        PawnDirty = 1,
    }

    public class NotificationsManager
    {
        private static List<INotification> notifications = new List<INotification>();
        private static MethodBase mPawnDirty = AccessTools.Method(typeof(NotificationsManager), nameof(NotificationsManager.Notify_PawnDirty));

        private static void Notify_PawnDirty(Pawn pawn)
        {
            pawn.Notify_Dirty();
        }

        public class INotification
        {
            public MethodBase method;
            public NotificationType type;
            public string packageId;

            private bool patched = false;
            private bool valid = true;
            private MethodInfo replacement;

            public bool Valid
            {
                get => valid;
            }

            public bool Patched
            {
                get => patched;
            }

            public INotification(string packageId, string type, MethodBase method)
            {
                this.packageId = packageId;
                this.method = method;
                if (type == "PawnDirty")
                    this.type = NotificationType.PawnDirty;
                else
                {
                    Log.Error($"ROCKETMAN: {packageId} doesn't implement proper notification type system!");
                    valid = false;
                }
            }

            public void Hook()
            {
                if (replacement != null || !valid || patched)
                    return;
                try
                {
                    HarmonyMethod prefix = GetPrefix(type);
                    replacement = Finder.Harmony.Patch(method, prefix: prefix);
                    patched = true;

                    if (RocketDebugPrefs.Debug) Log.Message($"ROCKETMAN: Notification by {packageId} added [{method.GetMethodPath()}][Patch Okay!]");
                }
                catch (Exception er)
                {
                    if (RocketDebugPrefs.Debug) Log.Error($"ROCKETMAN: {packageId} notification method of type {type} doesn't match the documenation! {er}");
                }
            }
        }

        public static void Register(string packageId, string type, MethodBase method)
        {
            INotification notification = new INotification(packageId, type, method);
            notifications.Add(notification);
        }

        public static void HookAll()
        {
            foreach (INotification notification in notifications)
                notification.Hook();
        }

        private static HarmonyMethod GetPrefix(NotificationType type)
        {
            if (type == NotificationType.PawnDirty)
                return new HarmonyMethod((MethodInfo)mPawnDirty);
            throw new NotImplementedException();
        }
    }
}
