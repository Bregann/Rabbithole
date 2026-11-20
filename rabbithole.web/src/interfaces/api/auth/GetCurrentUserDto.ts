export interface GetCurrentUserDto {
  id: number
  username: string
  isAdmin: boolean
  listenBrainzUsername: string | null
}
