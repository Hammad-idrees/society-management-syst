using SocietyMS.Models;

namespace SocietyMS.Helpers
{
    /// <summary>Maintains the currently logged-in user across the application session.</summary>
    public static class SessionManager
    {
        public static User CurrentUser { get; private set; }
        public static bool IsLoggedIn { get { return CurrentUser != null; } }

        /// <summary>Sets the current session user after successful login.</summary>
        public static void Login(User user)
        {
            CurrentUser = user;
        }

        /// <summary>Clears the session on logout.</summary>
        public static void Logout()
        {
            CurrentUser = null;
        }
    }
}

