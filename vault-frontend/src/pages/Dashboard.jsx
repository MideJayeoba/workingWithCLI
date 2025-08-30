import {useState, useEffect} from "react";
import NavBar from "../components/NavBar";
import FileList from "../components/FileList";
import FileUpload from "../components/FileUpload";
import "../App.css";

export default function Dashboard() {
    const [loading, setLoading] = useState(true);
    const [user, setUser] = useState({});
    const [error, setError] = useState("");
    const [files, setFiles] = useState([]);

    // use useEffect to fetch the user details from the database and store pass it on
    useEffect(() => {
        const fetchUserDetails = async () => {
            try {
                const response = await fetch("http://localhost:5280/user/Me", {
                    method: "GET",
                    headers: {
                        "Content-Type": "application/json",
                    },
                });
                if (!response.ok) {
                    throw new Error("Failed to fetch user details");
                }
                const data = await response.json();
                setUser(data.data);
                console.log("Fetched user details:", data); 
            } catch (error) {
                console.error("Error fetching user details:", error);
            }
        };
        fetchUserDetails();
    }, []);

  useEffect(() => {
  const fetchFiles = async () => {
    try {
      setLoading(true);
      setError("");
      const response = await fetch("http://localhost:5280/file/list", {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
      });
      if (!response.ok) {
        throw new Error(data.message || "Failed to fetch files");
      }
      const data = await response.json();
      setFiles(data);
    } catch (error) {
        console.error("Error fetching files:", error);
        setError("Failed to fetch files");
    } finally {
        setLoading(false);
    }
    };
    fetchFiles();
    }, []);

    const totalFiles = files.length;

    // map all the file sizes add them together and convert it to MB or GB as the case may be
    const storageUsed = (files.reduce((acc, file) => acc + file.Size, 0) / (1024 * 1024)).toFixed(2);
    if (storageUsed > 1024){
        storageUsed = (storageUsed / 1024).toFixed(2);
    }
    const storageUnit = storageUsed > 1024 ? "GB" : "MB"; 

    return (
        <div className="min-h-screen bg-gray-50">
            <NavBar user={user} />
            <main className="max-w-7xl mx-auto px-4 py-8">
                <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
                    <div className="bg-white p-6 rounded-lg shadow text-center">
                        <div className="text-lg font-semibold text-gray-700">Total Files</div>
                        <div className="text-3xl font-bold text-teal-600">{totalFiles}</div>
                    </div>
                    <div className="bg-white p-6 rounded-lg shadow text-center">
                        <div className="text-lg font-semibold text-gray-700">Storage Used</div>
                        <div className="text-3xl font-bold text-teal-600">{storageUsed} {storageUnit}</div>
                    </div>
                    <div className="bg-white p-6 rounded-lg shadow text-center">
                        <div className="text-lg font-semibold text-gray-700">Features coming soon</div>
                        <div className="text-3xl font-bold text-teal-600">3</div>
                    </div>
                </div>

                <div className="mb-8">
                    <FileUpload/>
                </div>

                <div className="bg-white rounded-lg shadow p-6">
                    <h2 className="text-xl font-bold mb-4 text-gray-800">Your Files</h2>
                    <FileList files={files} loading={loading} error={error} />
                </div>
            </main>
        </div>
    );
}