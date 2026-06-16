using HutechStore.Models;

namespace HutechStore.Helpers;

public static class SessionHelper
{
    public static void SetUser(ISession session, User user)
    {
        session.SetInt32("UserId",   user.Id);
        session.SetString("UserName", user.Name);
        session.SetString("UserRole", user.Role);
        session.SetString("UserEmail", user.Email);
        session.SetString("UserAvatar", user.Avatar ?? "");
        session.SetString("AuthProvider", user.AuthProvider);
    }

    public static void Clear(ISession session) => session.Clear();

    public static bool IsLoggedIn(ISession session) =>
        session.GetInt32("UserId").HasValue;

    public static bool IsAdmin(ISession session) =>
        session.GetString("UserRole") == "admin";

    public static int? GetUserId(ISession session) =>
        session.GetInt32("UserId");

    public static string GetUserName(ISession session) =>
        session.GetString("UserName") ?? "";

    public static string GetUserAvatar(ISession session) =>
        session.GetString("UserAvatar") ?? "";

    public static string GetAuthProvider(ISession session) =>
        session.GetString("AuthProvider") ?? "local";
}
