import { LoginPage } from './modules/identity/pages/LoginPage'
import { DashboardPage } from './modules/ledger/pages/DashboardPage'

function App() {
  // Check if we have a token to consider the user authenticated
  const isAuthenticated = !!localStorage.getItem('token');

  return (
    <div className="app-container">
      {!isAuthenticated ? (
        <LoginPage />
      ) : (
        <DashboardPage />
      )}
    </div>
  )
}

export default App
