export class City{
    constructor(x, y, name) {
        this.x = x;
        this.y = y;
        this.name = name;
    }
}

export function initializeCities(){
    let cities = []
    let Calgary = new City(180,62,'Calgary')
    let Vancouver = new City(83,75,'Vancouver')
    let Seattle = new City(79,114,'Seattle')
    let Portland = new City(63,151,'Portland')
    let SanFrancisco = new City(53,293,'SanFrancisco')
    let LosAngeles = new City(111,368,'LosAngeles')
    let LasVegas = new City(159,326,'LasVegas')
    let SaltLakeCity = new City(202, 247, 'SaltLakeCity')
    let Helena = new City(255, 158, 'Helena')
    let Winipeg = new City(348, 70, 'Winnipeg')
    let Duluth = new City(433, 153, 'Duluth')
    let SantaFe = new City(294, 335, 'SantaFe')
    let Denver = new City(299, 270, 'Denver')
    let Omaha = new City(410, 219, 'Omaha')
    let KansasCity = new City(426, 257, 'KansasCity')
    let OklahomaCity = new City(411, 319, 'OklahomaCity')
    let Dallas = new City(426, 383, 'Dallas')
    let ElPaso = new City(290, 400, 'ElPaso')
    let Phoenix = new City(201, 373, 'Phoenix')
    let Houston = new City(457, 412, 'Houston')
    let LittleRock = new City(478, 322, 'LittleRock')
    let NewOrleans = new City(527, 403, 'NewOrleans')
    let Nashville = new City(561, 286, 'Nashville')
    let SaintLouis = new City(490, 257, 'SaintLouis')
    let Atlanta = new City(600, 311, 'Atlanta')
    let Miami = new City(694, 429, 'Miami')
    let Charleston = new City(669, 315, 'Charleston')
    let Raleigh = new City(648, 269, 'Raleigh')
    let Chicago = new City(526, 198, 'Chicago')
    let SaultStMarie = new City(528, 106, 'SaultSaintMarie')
    let Toronto = new City(611, 121, 'Toronto')
    let Pittsburgh = new City(623, 188, 'Pittsburgh')
    let NewYork = new City(687, 155, 'NewYork')
    let Washington = new City(692, 220, 'Washington')
    let Montreal = new City(672, 59, 'Montreal')
    let Boston = new City(726, 102, 'Boston')

    cities.push(Calgary)
    cities.push(Vancouver)
    cities.push(Seattle)
    cities.push(Portland)
    cities.push(SanFrancisco)
    cities.push(LosAngeles)
    cities.push(LasVegas)
    cities.push(SaltLakeCity)
    cities.push(Helena)
    cities.push(Winipeg)
    cities.push(Duluth)
    cities.push(SantaFe)
    cities.push(Denver)
    cities.push(Omaha)
    cities.push(KansasCity)
    cities.push(OklahomaCity)
    cities.push(Dallas)
    cities.push(ElPaso)
    cities.push(Phoenix)
    cities.push(Houston)
    cities.push(LittleRock)
    cities.push(NewOrleans)
    cities.push(Nashville)
    cities.push(SaintLouis)
    cities.push(Atlanta)
    cities.push(Miami)
    cities.push(Charleston)
    cities.push(Raleigh)
    cities.push(Chicago)
    cities.push(SaultStMarie)
    cities.push(Toronto)
    cities.push(Pittsburgh)
    cities.push(NewYork)
    cities.push(Washington)
    cities.push(Montreal)
    cities.push(Boston)

    cities.sort((a, b) => a.name.localeCompare(b.name)); 
    return cities;
}