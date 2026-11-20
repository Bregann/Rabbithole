import { BrowserRouter, Route, Routes } from 'react-router'
import ClientLayout from '@/components/navigation/ClientLayout'
import ProtectedRoute from '@/components/navigation/ProtectedRoute'
import ArtistComponent from '@/components/pages/ArtistComponent'
import DiscoverComponent from '@/components/pages/DiscoverComponent'
import LoginComponent from '@/components/pages/LoginComponent'
import NotFoundComponent from '@/components/pages/NotFoundComponent'
import PlaylistComponent from '@/components/pages/PlaylistComponent'
import PlaylistsComponent from '@/components/pages/PlaylistsComponent'
import RabbitholeComponent from '@/components/pages/RabbitholeComponent'
import RequestsComponent from '@/components/pages/RequestsComponent'
import SearchComponent from '@/components/pages/SearchComponent'
import SettingsComponent from '@/components/pages/SettingsComponent'

// Route table only. Each route points at a components/pages/<X>Component.
export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginComponent />} />
        <Route element={<ProtectedRoute />}>
          <Route element={<ClientLayout />}>
            <Route index element={<DiscoverComponent />} />
            <Route path="search" element={<SearchComponent />} />
            <Route path="artist/:artistId" element={<ArtistComponent />} />
            <Route path="rabbithole/:artistId" element={<RabbitholeComponent />} />
            <Route path="playlists" element={<PlaylistsComponent />} />
            <Route path="playlists/:playlistId" element={<PlaylistComponent />} />
            <Route path="requests" element={<RequestsComponent />} />
            <Route path="settings" element={<SettingsComponent />} />
            <Route path="*" element={<NotFoundComponent />} />
          </Route>
        </Route>
      </Routes>
    </BrowserRouter>
  )
}
