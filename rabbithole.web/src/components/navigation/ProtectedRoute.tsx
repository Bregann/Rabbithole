import { Navigate, Outlet, useLocation } from 'react-router'
import PageLoader from '@/components/common/PageLoader'
import { useAuth } from '@/context/authContext'

export default function ProtectedRoute() {
  const { user, isLoading } = useAuth()
  const location = useLocation()

  if (isLoading) {
    return <PageLoader />
  }

  if (user === null) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />
  }

  return <Outlet />
}
