export enum ParticipantStatus {
  Disconnected = 0,
  Connecting = 1,
  Connected = 2,
  Speaking = 3,
  Muted = 4,
  VideoOff = 5
}

export enum CallType {
  Audio = 0,
  Video = 1
}

export enum GroupType {
  Chat = 0,
  VideoCall = 1
}

export enum MessageType {
  Text = 0,
  System = 1,
  File = 2,
  Image = 3,
  CallStarted = 4,
  CallEnded = 5,
  UserJoined = 6,
  UserLeft = 7
}
