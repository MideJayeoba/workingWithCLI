import { useNavigate } from "react-router-dom";

export default function NavBar({ user }) {
    const navigate = useNavigate();
    const handleLogout = async () => {
        // Optionally call backend logout endpoint
		try {
			const response = await fetch("http://localhost:5280/user/logout", {
				method: "POST",
				headers: {
					"Content-Type": "application/json"
				}
			});
			navigate("/");
			if (!response.ok) {
				throw new Error("Logout failed");
			}
		} catch (error) {
			console.error("Error logging out:", error);
		}
    };
    const avatar = user?.avatar || "https://ui-avatars.com/api/?name=" + encodeURIComponent(user?.Name);
	
	return (
		<header className="w-full bg-white border-b shadow-sm">
				<div className="max-w-7xl mx-auto px-4 py-4 flex justify-between items-center">
                    <div className="flex items-center gap-4">
                        <img src={avatar} alt="avatar" className="w-10 h-10 rounded-full border" />
                        <div>
                            <div className="font-bold text-gray-800 capitalize">{user.Name}</div>
                        </div>
                    </div>
					<div className="text-xl font-bold text-teal-700 tracking-wide">FileVault</div>
                    <button onClick={handleLogout}
					className="px-4 py-2 bg-teal-600 text-white rounded hover:bg-teal-700">Logout</button>
                </div>
		</header>
	);
}
