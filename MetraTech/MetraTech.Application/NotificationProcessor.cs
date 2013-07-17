﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using MetraTech.Domain.DataAccess;
using MetraTech.Domain.Events;
using MetraTech.Domain.Notifications;

namespace MetraTech.Application
{
    public static class NotificationProcessor
    {
      /// <summary>
      /// Determines which notifications should be delivered for a given event, and delivers those notifications using
      /// the data from that same event
      /// </summary>
      /// <param name="context">A database context used to query notification configurations</param>
      /// <param name="eventInstance">The event to be evaluated and used for rendering</param>
      public static void ProcessEvent(IMetraNetContext context, Event eventInstance)
      {
        if (context == null) throw new ArgumentNullException("context");
        if (eventInstance == null) throw new ArgumentNullException("eventInstance");

        var notifications = RetrieveNotificationsForEvent(context, eventInstance);
        DeliverNotificationsForEvent(eventInstance, notifications);
      }

      /// <summary>
      /// Delivers notifications using data from the event provided
      /// </summary>
      /// <param name="eventInstance">The event to be used to render the notifications</param>
      /// <param name="notifications">The notifications to be delivered</param>
      public static void DeliverNotificationsForEvent(Event eventInstance, IEnumerable<NotificationConfiguration> notifications)
      {
        if (eventInstance == null) throw new ArgumentNullException("eventInstance");
        if (notifications == null) throw new ArgumentNullException("notifications");

        foreach (var notification in notifications)
        {
          var fromAddress = new MailAddress("mdesousa@metratech.com");
            //TODO: Retrieve appropriate address based on organization
          var message = notification.EmailTemplate.CreateMailMessage(eventInstance, fromAddress, null);
          EmailProcessor.SendEmail(notification.NotificationEndpoint, message);
        }
      }

      /// <summary>
      /// Determines which Notifications should be delivered for a given event
      /// </summary>
      /// <param name="context">A database context used to query notification configurations</param>
      /// <param name="eventInstance">The event to be evaluated</param>
      /// <returns>A list of notifications that should be triggered</returns>
      public static IEnumerable<NotificationConfiguration> RetrieveNotificationsForEvent(IMetraNetContext context, Event eventInstance)
      {
        if (context == null) throw new ArgumentNullException("context");
        if (eventInstance == null) throw new ArgumentNullException("eventInstance");

        var eventType = eventInstance.GetType().Name;
        var notifications = context.NotificationConfigurations.Where(x => x.EventType == eventType).ToList();
        return notifications;
      }
    }
}
