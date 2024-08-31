using EasyUI.Toast;
using UnityEngine;

namespace GimJem.Utilities
{
    public static class ToastUtil
    {
        public static void ShowErrorToast(string message)
        {
            Toast.Show($"{message}", 2f, ToastColor.Red);
            Debug.LogError($"Error Toast: {message}");
        }

        // Add this new method to show success Toast
        public static void ShowSuccessToast(string message)
        {
            Toast.Show(message, 2f, ToastColor.Green);
            Debug.Log($"Success Toast: {message}");
        }
    }
}