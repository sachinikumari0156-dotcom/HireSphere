import "./CandidateDashboard.css";
import { useEffect, useState } from "react";
import { API_BASE_URL } from "../api/config";


function CandidateDashboard() {


    const user = JSON.parse(
        localStorage.getItem("user")
    );


    const token = localStorage.getItem("token");



    const [jobs, setJobs] = useState([]);

    const [applications, setApplications] = useState([]);






    useEffect(() => {


        const loadJobs = async () => {


            try {


                const response = await fetch(
                    `${API_BASE_URL}/Jobs`
                );


                if (response.ok) {


                    const data = await response.json();

                    setJobs(data);


                }
                else {

                    console.log("Jobs loading failed");

                }



            }
            catch (error) {


                console.log("Jobs Error:", error);


            }


        };








        const loadApplications = async () => {


            try {


                const response = await fetch(

                    `${API_BASE_URL}/Applications/MyApplications`,

                    {

                        method: "GET",

                        headers: {

                            "Authorization":
                                `Bearer ${token}`

                        }


                    }

                );



                if (response.ok) {


                    const data = await response.json();


                    setApplications(data);


                }



            }
            catch (error) {


                console.log(
                    "Applications Error:",
                    error
                );


            }


        };







        loadJobs();

        loadApplications();




    }, [token]);









    const applyJob = async (jobId) => {



        try {



            const response = await fetch(

                `${API_BASE_URL}/Applications`,

                {


                    method: "POST",


                    headers: {


                        "Content-Type":
                            "application/json",


                        "Authorization":
                            `Bearer ${token}`


                    },



                    body: JSON.stringify({


                        jobId: jobId,


                        coverLetter:
                            "I am interested in this job."


                    })



                }


            );






            if (response.ok) {


                alert(
                    "Job applied successfully!"
                );



                window.location.reload();



            }
            else {


                const error =
                    await response.text();


                alert(error);


            }



        }
        catch (error) {


            console.log(
                "Apply Error:",
                error
            );


        }



    };











    return (

        <div className="candidate-page">



            <div className="candidate-header">


                <h1>

                    Welcome, {user?.fullName}

                </h1>



                <p>

                    Find your dream job and manage your applications.

                </p>



            </div>









            <div className="dashboard-cards">



                <div className="dash-card">


                    <h2>

                        {applications.length}

                    </h2>


                    <p>

                        Applied Jobs

                    </p>


                </div>








                <div className="dash-card">


                    <h2>

                        0

                    </h2>


                    <p>

                        Saved Jobs

                    </p>


                </div>








                <div className="dash-card">


                    <h2>

                        0

                    </h2>


                    <p>

                        Interviews

                    </p>


                </div>





            </div>












            <div className="jobs-section">



                <h2>

                    Latest Jobs

                </h2>








                {
                    jobs.length === 0 ?



                        (

                            <p>

                                No jobs available

                            </p>

                        )



                        :



                        jobs.map((job) => (



                            <div
                                className="job-box"
                                key={job.id}
                            >




                                <h3>

                                    {job.title}

                                </h3>





                                <p>

                                    {job.description}

                                </p>





                                <p>

                                    Skills:
                                    {" "}
                                    {job.requiredSkills}

                                </p>





                                <p>

                                    Location:
                                    {" "}
                                    {job.location}

                                </p>






                                <button
                                    onClick={() =>
                                        applyJob(job.id)
                                    }
                                >

                                    Apply Now

                                </button>





                            </div>



                        ))

                }





            </div>






        </div>


    );

}



export default CandidateDashboard;