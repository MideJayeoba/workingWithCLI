
import { useState } from "react";

export default function FileUpload() {
  const [isLoading, setIsLoading] = useState(false);
  const [filepath, setFilepath] = useState("");
  const [error, setError] = useState("");
  const [parentId, setParentId] = useState(null);

  const handleUploadClick = async () => {
    try {
      setIsLoading(true);
      setError("");
      const res = await fetch("http://localhost:5280/file/file", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ filepath, parentId})
      });
      if (!res.ok) {
        const errorData = await res.json();
        alert("Upload failed: " + errorData.message);
        throw new Error(errorData.message);
      }
      if (res.ok) {
        alert("File uploaded successfully");
        setFilepath("");
        setParentId(null);
      }
    } catch (err) {
      console.error("Error uploading file:", err);
      setError(err.message);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="p-4 border-b-green-100 shadow-2xl shadow-cyan-50 rounded-lg w-full max-w-lg h-50 bg-white">
      <div className="flex flex-col gap-5">
        <input
          type="text"
          placeholder="File Path"
          required
          value={filepath}
          onChange={e => setFilepath(e.target.value)}
          className="block w-full h-10 p-2 text-sm text-gray-700 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-teal-500"
        />
        <input
          type="text"
          placeholder="Parent ID"
          value={parentId}
          onChange={e => setParentId(e.target.value)}
          className="block w-full h-10 p-2 text-sm text-gray-700 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-teal-500"
        />
        {error && <div className="text-red-500 text-sm">{error}</div>}
        <button
          className={`py-2 px-4 bg-teal-600 text-white rounded font-semibold shadow hover:bg-teal-700 transition-all duration-150 ${isLoading || !filepath ? "opacity-50 cursor-not-allowed" : ""}`}
          onClick={handleUploadClick}
          disabled={isLoading || !filepath}
        >
          {isLoading ? "Uploading..." : "Upload"}
        </button>
      </div>
    </div>
  );
}
