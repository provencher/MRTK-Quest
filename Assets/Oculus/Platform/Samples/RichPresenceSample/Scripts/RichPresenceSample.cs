// Uncomment this if you have the Touch controller classes in your project
//#define USE_OVRINPUT

using Oculus.Platform;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * This class shows a very simple way to integrate setting the Rich Presence
 * with a destination and how to respond to a user's app launch details that
 * include the destination they wish to travel to.
 */
public class RichPresenceSample : MonoBehaviour
{
  /**
   * Sets extra fields on the rich presence
   */

  // Optional message to override the deep-link message set in the developer
  // dashboard. This is where you can specify the ID for a room, party,
  // matchmaking pool or group server. There should be no whitespaces.
  public string DeeplinkMessageOverride;

  // A boolean to indicate whether the destination is joinable. You can check
  // the current capacity against the max capacity to determine whether the room
  // is joinable.
  public bool IsJoinable;

  // A boolean to indicate whether the current user is idling in the app.
  public bool IsIdle;

  // The current capacity at that destination. Used for displaying with the
  // extra context when it's set to RichPresenceExtraContext.CurrentCapacity
  public uint CurrentCapacity;

  // The maximum capacity of the destination. Can be used with current capacity
  // to see if a user can join. For example, when used with a room, set the max
  // capacity of the destination to the max capacity of the room.
  // Used for displaying with the extra context when it's set to
  // RichPresenceExtraContext.CurrentCapacity
  public uint MaxCapacity;

  // The time the current match starts or started. Used for displaying with the
  // extra context when it's set to RichPresenceExtraContext.StartedAgo
  public System.DateTime StartTime = System.DateTime.Now;

  // The time the current match ends. Used for displaying with the
  // extra context when it's set to RichPresenceExtraContext.EndingIn
  public System.DateTime EndTime = System.DateTime.Now.AddHours(2);

  // Extra information to set the user’s presence correctly. This should give
  // more insight for people to decided whether or not to join the user.
  public RichPresenceExtraContext ExtraContext = RichPresenceExtraContext.LookingForAMatch;

  public Text InVRConsole;
  public Text DestinationsConsole;

  private List<string> DestinationAPINames = new List<string>();
  private ulong LoggedInUserID = 0;

  // Start is called before the first frame update
  void Start()
  {
    UpdateConsole("Init Oculus Platform SDK...");
    Core.AsyncInitialize().OnComplete(message => {
      if (message.IsError)
      {
        // Init failed, nothing will work
        UpdateConsole(message.GetError().Message);
      }
      else
      {
        /**
         * Get the deeplink message when the app starts up
         */
        UpdateConsole("Init complete!\n" + GetAppLaunchDetails());

        /**
         * Get and cache the Logged in User ID for future queries
         */
        Users.GetLoggedInUser().OnComplete(OnLoggedInUser);

        /**
         * Get the list of destinations defined for this app from the developer portal
         */
        RichPresence.GetDestinations().OnComplete(OnGetDestinations);

        /**
         * Listen for future deeplink message changes that might come in
         */
        ApplicationLifecycle.SetLaunchIntentChangedNotificationCallback(OnLaunchIntentChangeNotif);
      }
    });
  }

  /**
    * Setting the rich presence
    */
  void SetPresence()
  {
    var options = new RichPresenceOptions();

    // Only Destination API Name is required
    options.SetApiName(DestinationAPINames[DestinationIndex]);

    // Override the deeplink message if you like, otherwise it will use the one found in the destination
    if (!string.IsNullOrEmpty(DeeplinkMessageOverride))
    {
      options.SetDeeplinkMessageOverride(DeeplinkMessageOverride);
    }

    // Set is Joinable to let other players deeplink and join this user via the presence
    options.SetIsJoinable(IsJoinable);

    // Set if the user is idle
    options.SetIsIdle(IsIdle);

    // Used when displaying the current to max capacity on the user's presence
    options.SetCurrentCapacity(CurrentCapacity);
    options.SetMaxCapacity(MaxCapacity);

    // Used to display how long since this start / when will this end
    options.SetStartTime(StartTime);
    options.SetEndTime(EndTime);

    // Used to display extra info like the capacity, start/end times, or looking for a match
    options.SetExtraContext(ExtraContext);
    UpdateConsole("Setting Rich Presence to " + DestinationAPINames[DestinationIndex] + " ...");

    // Here we are setting the rich presence then fetching it after we successfully set it
    RichPresence.Set(options).OnComplete(message => {
      if (message.IsError)
      {
        UpdateConsole(message.GetError().Message);
      }
      else
      {
        // Note that Users.GetLoggedInUser() does not do a server fetch and will
        // not get an updated presence status
        Users.Get(LoggedInUserID).OnComplete(message2 =>
        {
          if (message2.IsError)
          {
            UpdateConsole("Success! But rich presence is unknown!");
          }
          else
          {
            UpdateConsole("Rich Presence set to:\n" + message2.Data.Presence + "\n" + message2.Data.PresenceDeeplinkMessage + "\n" + message2.Data.PresenceDestinationApiName);
          }
        });
      }
    });
  }

  /**
    * Clearing the rich presence
    */
  void ClearPresence()
  {
    UpdateConsole("Clearing Rich Presence...");
    RichPresence.Clear().OnComplete(message => {
      if (message.IsError)
      {
        UpdateConsole(message.GetError().Message);
      }
      else
      {
        // Clearing the rich presence then fetching the user's presence afterwards
        Users.Get(LoggedInUserID).OnComplete(message2 =>
        {
          if (message2.IsError)
          {
            UpdateConsole("Rich Presence cleared! But rich presence is unknown!");
          }
          else
          {
            UpdateConsole("Rich Presence cleared!\n" + message2.Data.Presence + "\n");
          }
        });
      }
    });
  }

  /**
   * Getting the deeplink information off the app launch details. When a user requests
   * to travel to a destination from outside your app, their request will be found here
   * Get the info to bring the user to the expected destination.
   */
  string GetAppLaunchDetails()
  {
    var launchDetails = ApplicationLifecycle.GetLaunchDetails();

    // The other users this user expect to see after traveling to the destination
    // If there is conflicting data between the inputted users and destination,
    // favor using the users.
    // For example, if user A & destination 1 was passed in, but user A is now
    // in destination 2, it is better to bring the current user to destination 2
    // if possible.
    var users = launchDetails.UsersOptional;
    var usersCount = (users != null) ? users.Count : 0;

    // The deeplink message, this should give enough info on how to go the
    // destination in the app.
    var deeplinkMessage = launchDetails.DeeplinkMessage;

    // The API Name of the destination. You can set the user to this after
    // navigating to the app
    var destinationApiName = launchDetails.DestinationApiName;

    var detailsString = "-Deeplink Message:\n" + deeplinkMessage + "\n-Api Name:\n" + destinationApiName + "\n-Users:\n";
    if (usersCount > 0)
    {
      foreach(var user in users)
      {
        detailsString += user.OculusID + "\n";
      }
    } else
    {
      detailsString += "null\n";
    }
    detailsString += "\n";
    return detailsString;
  }

  // User has interacted with a deeplink outside this app
  void OnLaunchIntentChangeNotif(Oculus.Platform.Message<string> message)
  {
    if (message.IsError)
    {
      UpdateConsole(message.GetError().Message);
    } else
    {
      UpdateConsole("Updated launch details:\n" + GetAppLaunchDetails());
    }
  }

  void OnGetDestinations(Message<Oculus.Platform.Models.DestinationList> message)
  {
    if (message.IsError)
    {
      UpdateConsole("Could not get the list of destinations!");
    }
    else
    {
      foreach(Oculus.Platform.Models.Destination destination in message.Data)
      {
        DestinationAPINames.Add(destination.ApiName);
        UpdateDestinationsConsole();
      }
    }
  }

  #region Helper Functions

  private int DestinationIndex = 0;
  private bool OnlyPushUpOnce = false;
  // Update is called once per frame
  void Update()
  {
    if (PressAButton())
    {
      if (DestinationAPINames.Count > 0)
      {
        SetPresence();
      }
      else
      {
        UpdateConsole("No destinations to set to!");
        return;
      }
    }
    else if (PressBButton())
    {
      ClearPresence();
    }

    ScrollThroughDestinations();
  }

  private void ScrollThroughDestinations()
  {
    if (PressUp())
    {
      if (!OnlyPushUpOnce)
      {
        DestinationIndex--;
        if (DestinationIndex < 0)
        {
          DestinationIndex = DestinationAPINames.Count - 1;
        }
        OnlyPushUpOnce = true;
        UpdateDestinationsConsole();
      }
    }
    else if (PressDown())
    {
      if (!OnlyPushUpOnce)
      {
        DestinationIndex++;
        if (DestinationIndex >= DestinationAPINames.Count)
        {
          DestinationIndex = 0;
        }
        OnlyPushUpOnce = true;
        UpdateDestinationsConsole();
      }
    }
    else
    {
      OnlyPushUpOnce = false;
    }
  }

  private void UpdateDestinationsConsole()
  {
    if (DestinationAPINames.Count == 0)
    {
      DestinationsConsole.text = "Add some destinations to the developer dashboard first!";
    }
    string destinations = "Destination API Names:\n";
    for (int i = 0; i < DestinationAPINames.Count; i++)
    {
      if (i == DestinationIndex)
      {
        destinations += "==>";
      }
      destinations += DestinationAPINames[i] + "\n";
    }
    DestinationsConsole.text = destinations;
  }

  private void OnLoggedInUser(Message<Oculus.Platform.Models.User> message)
  {
    if (message.IsError)
    {
      Debug.LogError("Cannot get logged in user");
    }
    else
    {
      LoggedInUserID = message.Data.ID;
    }
  }

  private void UpdateConsole(string value)
  {
    Debug.Log(value);

    InVRConsole.text =
      "Scroll Up/Down on Right Thumbstick\n(A) - Set Rich Presence to selected\n(B) - Clear Rich Presence\n\n" + value;
  }

  #endregion

  #region I/O Inputs
  private bool PressAButton()
  {
#if USE_OVRINPUT
    return OVRInput.GetUp(OVRInput.Button.One) || Input.GetKeyUp(KeyCode.A);
#else
    return Input.GetKeyUp(KeyCode.A);
#endif
  }

  private bool PressBButton()
  {
#if USE_OVRINPUT
    return OVRInput.GetUp(OVRInput.Button.Two) || Input.GetKeyUp(KeyCode.B);
#else
    return Input.GetKeyUp(KeyCode.B);
#endif
  }

  private bool PressUp()
  {
#if USE_OVRINPUT
    Vector2 axis = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
    return (axis.y > 0.2 || Input.GetKeyUp(KeyCode.UpArrow));
#else
    return Input.GetKeyUp(KeyCode.UpArrow);
#endif
  }

  private bool PressDown()
  {
#if USE_OVRINPUT
    Vector2 axis = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
    return (axis.y < -0.2 || Input.GetKeyUp(KeyCode.DownArrow));
#else
    return Input.GetKeyUp(KeyCode.DownArrow);
#endif
  }

  #endregion
}
