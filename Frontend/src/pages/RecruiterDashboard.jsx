function RecruiterDashboard() {


    const user = JSON.parse(
        localStorage.getItem("user")
    );


    return (

        <div>


            <h1>
                Welcome {user?.fullName}
            </h1>


            <h2>
                Recruiter Dashboard
            </h2>


            <p>
                Create jobs and manage applicants.
            </p>


        </div>

    );

}


export default RecruiterDashboard;