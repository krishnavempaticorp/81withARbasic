CREATE TABLE t_Notification_Event_Consumers
(
	id_not_evnt_consumer number(10) NOT NULL,
	id_notification_event number(10) NOT NULL,
	id_acc number(10) NOT NULL,
  CONSTRAINT PK_t_Notification_Evnt_Consmrs PRIMARY KEY (id_not_evnt_consumer),
  CONSTRAINT FK_t_Notification_Evnt_Consmrs FOREIGN KEY (id_notification_event) REFERENCES t_Notification_Events(id_notification_event)
)