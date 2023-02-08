using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QPathFinder
{
	public static class Logger
	{
		
		public enum Level 
		{
			Info = 1,    // Prints everything 
			Warnings = 2,   // Prints warnings and errors
			Errors = 3,      // prints only errors
			None = 4
		}

        public static void SetLoggingLevel ( QPathFinder.Logger.Level level ) 
		{ 
			m_logLevel = level; 
		}

		public static bool CanLogWarning { get { return m_logLevel <= Level.Warnings || IsRunningInEditorMode; }}
		public static void LogWarning ( string message, bool includeTimeStamp = false )
		{
			Log( Level.Warnings, message, includeTimeStamp );
		}

		public static bool CanLogError { get { return m_logLevel <= Level.Errors || IsRunningInEditorMode; }}
		public static void LogError ( string message, bool includeTimeStamp = false )
		{
			Log( Level.Errors, message, includeTimeStamp );
		}

		public static bool CanLogInfo { get { return m_logLevel <= Level.Info || IsRunningInEditorMode; }}
		public static void LogInfo ( string message, bool includeTimeStamp = false )
		{
			Log( Level.Info, message, includeTimeStamp );
		}

		public static void Log ( QPathFinder.Logger.Level level, string message, bool includeTimeStamp = false )
        {
			bool isEditorMode = IsRunningInEditorMode;
			if ( includeTimeStamp )
				message = "[Time:" + Time.realtimeSinceStartup + "]" + message;

            if ( level == QPathFinder.Logger.Level.Info )
            {
				if ( m_logLevel <= level || isEditorMode )
					Debug.Log("[QPathFinder:Info] " + message);
            }
            else if ( level == QPathFinder.Logger.Level.Warnings )
            {
				if ( m_logLevel <= level || isEditorMode )
					Debug.LogWarning("[QPathFinder:Warn] " + message);
            }
            else if ( level == QPathFinder.Logger.Level.Errors )
            {
				if ( m_logLevel <= level || isEditorMode )
					Debug.LogError("[QPathFinder:Err] " + message);
            }
        }

		public static void SetDebugDrawLineDuration ( float duration )
		{
			DrawLineDuration = duration;
		}

		public static float DrawLineDuration { get; private set; }

		private static bool IsRunningInEditorMode { get { return !Application.isPlaying; }}
        private static QPathFinder.Logger.Level m_logLevel = Level.Warnings ;
        
	}
}
