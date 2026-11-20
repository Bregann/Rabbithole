/* eslint-disable no-unused-vars */
// Mirrors Rabbithole.Domain.Enums.RequestStatus (serialised as numbers)
export enum RequestStatus {
  Pending = 0,
  WaitingForLidarr = 1,
  Searching = 2,
  Available = 3,
  Failed = 4,
}
