export const postLogin = (username, password) => {
    const baseurl = "https://lvlupcs.azurewebsites.net/"
    const url = baseurl + "ValidateUserLogin";
    const data = {
        "username": username,
        "password": password
    };

    return fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
    })
    .then(response => response.json())
    .catch(error => {
        console.error('Error:', error);
    });

};

function organizeTasksByTree(tasks) {
    // Create objects to hold tasks for each tree.
    const trees = {
        Strength: [],
        Social: [],
        Intellectual: []
    };

    // Populate trees with tasks.
    tasks.forEach(task => {
        if (task.treeid in trees){
            trees[task.treeid].push(task);
        }
    });

    // Sort each tree by xp values in ascending order.
    for (const tree in trees) {
        trees[tree].sort((a, b) => a.xp - b.xp);
    }

    return trees;
}