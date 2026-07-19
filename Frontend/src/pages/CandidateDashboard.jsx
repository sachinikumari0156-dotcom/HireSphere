function CandidateDashboard() {

    const user = JSON.parse(
        localStorage.getItem("user")
    );


    return (

        <div>

            <h1>
                Welcome {user?.fullName}
            </h1>

            <h2>
                Candidate Dashboard
            </h2>

            <p>
                Find jobs, apply and manage your applications.
            </p>


        </div>

    );

}


export default CandidateDashboard;